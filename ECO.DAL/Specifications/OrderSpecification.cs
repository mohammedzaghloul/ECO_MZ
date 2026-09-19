using ECO.DAL.Entities.OrderEntities;
using System.Linq.Expressions;

namespace ECO.DAL.Specifications
{
    public class OrderSpecification : BaseSpecification<Order>
    {
        public OrderSpecification()
        {
            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
            ApplyOrderByDescending(order => order.OrderDate);
        }

        public OrderSpecification(string buyerEmail)
        {
            ApplyCriteria(order => order.BuyerEmail == buyerEmail);
            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
            ApplyOrderByDescending(order => order.OrderDate);
        }

        public OrderSpecification(string buyerEmail, string basketId)
        {
            ApplyCriteria(order => order.BuyerEmail == buyerEmail && order.BasketId == basketId);
            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
            ApplyOrderByDescending(order => order.OrderDate);
        }

        public static OrderSpecification ByPaymentIntentId(string paymentIntentId)
        {
            var specification = new OrderSpecification();
            specification.ApplyCriteria(order => order.PaymentIntentId == paymentIntentId);
            return specification;
        }

        public static OrderSpecification VerifiedPurchaseForProduct(string buyerEmail, int productId)
        {
            var specification = new OrderSpecification();
            specification.ApplyCriteria(order =>
                order.BuyerEmail == buyerEmail &&
                order.Status == Status.PaymentRecevied &&
                order.OrderItems.Any(item => item.ProductItemId == productId.ToString()));
            return specification;
        }

        public OrderSpecification(int id, string buyerEmail)
        {
            ApplyCriteria(order => order.Id == id && order.BuyerEmail == buyerEmail);
            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
            ApplyOrderByDescending(order => order.OrderDate);
        }

        /// <summary>Admin: any order by id, no email restriction.</summary>
        public OrderSpecification(int id, bool byId)
        {
            ApplyCriteria(order => order.Id == id);
            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
        }

        /// <summary>Admin: paged listing with optional status filter and buyer-email search.</summary>
        public OrderSpecification(
            string? status,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Status>(status, ignoreCase: true, out var parsed))
            {
                var matched = parsed;
                ApplyCriteria(order => order.Status == matched);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                AddCriteria(order => order.BuyerEmail.ToLower().Contains(term));
            }

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                AddCriteria(order => order.OrderDate >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                AddCriteria(order => order.OrderDate < end);
            }

            AddInclude(order => order.DeliveryMethod);
            AddInclude(order => order.OrderItems);
            ApplyOrderByDescending(order => order.OrderDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }

        private void AddCriteria(Expression<Func<Order, bool>> criteria)
        {
            Criteria = Criteria is null
                ? criteria
                : AndAlso(Criteria, criteria);
        }

        private static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body)!;
            var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(leftBody, rightBody), parameter);
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression from;
            private readonly ParameterExpression to;

            public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
            {
                this.from = from;
                this.to = to;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == from ? to : base.VisitParameter(node);
        }
    }
}
