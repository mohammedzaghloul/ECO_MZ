using ECO.DAL.Entities.Product;
using System.Linq.Expressions;

namespace ECO.DAL.Specifications
{
    public class CategorySpecification : BaseSpecification<Category>
    {
        public CategorySpecification()
        {
            AddInclude(category => category.Products);
            AddInclude(category => category.ParentCategory);
        }

        public CategorySpecification(int id)
        {
            ApplyCriteria(category => category.Id == id);
            AddInclude(category => category.Products);
            AddInclude(category => category.ParentCategory);
        }

        public CategorySpecification WithIncludes(IEnumerable<Expression<Func<Category, object>>> includes)
        {
            foreach (var include in includes)
            {
                AddInclude(include);
            }

            return this;
        }
    }
}
