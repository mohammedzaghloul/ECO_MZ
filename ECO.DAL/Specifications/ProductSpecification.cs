using ECO.DAL.Entities.Product;
using ECO.DAL.Sharing;
using System.Linq.Expressions;

namespace ECO.DAL.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification()
        {
            AddInclude(product => product.Category);
            AddInclude(product => product.Photos);
            AddInclude(product => product.Reviews);
            AddInclude(product => product.Specifications);
            ApplyOrderBy(product => product.Name);
        }

        public ProductSpecification(int id)
            : base(product => product.Id == id)
        {
            AddInclude(product => product.Category);
            AddInclude(product => product.Photos);
            AddInclude(product => product.Reviews);
            AddInclude(product => product.Specifications);
        }

        public ProductSpecification(IEnumerable<Expression<Func<Product, object>>> includes)
        {
            foreach (var include in includes)
            {
                AddInclude(include);
            }

            ApplyOrderBy(product => product.Name);
        }

        public ProductSpecification(ProductParams? productParams)
        {
            AddInclude(product => product.Category);
            AddInclude(product => product.Photos);
            AddInclude(product => product.Reviews);
            AddInclude(product => product.Specifications);

            var searchTerm = productParams?.Search ?? productParams?.Serach;

            if (productParams?.CategoryId.HasValue == true)
            {
                ApplyCriteria(product => product.CategoryId == productParams.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch = searchTerm.Trim();
                var searchExpression = (Expression<Func<Product, bool>>)(product =>
                    product.Name.ToLower().Contains(normalizedSearch.ToLower()) ||
                    product.Description.ToLower().Contains(normalizedSearch.ToLower()));

                if (Criteria is null)
                {
                    ApplyCriteria(searchExpression);
                }
                else
                {
                    var currentCriteria = Criteria;
                    ApplyCriteria(product => currentCriteria!.Compile()(product) && searchExpression.Compile()(product));
                }
            }

            if (!string.IsNullOrEmpty(productParams?.Sort))
            {
                switch (productParams.Sort)
                {
                    case "name":
                        ApplyOrderBy(product => product.Name);
                        break;
                    case "priceAsc":
                    case "Asce":
                        ApplyOrderBy(product => product.NewPrice);
                        break;
                    case "priceDesc":
                    case "Desc":
                        ApplyOrderByDescending(product => product.NewPrice);
                        break;
                    case "bestSelling":
                        ApplyOrderBy(product => product.Name);
                        break;
                    default:
                        ApplyOrderBy(product => product.Name);
                        break;
                }
            }
            else
            {
                ApplyOrderBy(product => product.Name);
            }

            if (productParams?.PageNumber.HasValue == true && productParams?.PageSize.HasValue == true)
            {
                var skip = (productParams.PageNumber.Value - 1) * productParams.PageSize.Value;
                ApplyPaging(skip, productParams.PageSize.Value);
            }
        }

        public ProductSpecification WithIncludes(IEnumerable<Expression<Func<Product, object>>> includes)
        {
            foreach (var include in includes)
            {
                AddInclude(include);
            }

            return this;
        }
    }
}
