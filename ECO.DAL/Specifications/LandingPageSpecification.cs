using ECO.DAL.Entities.Landing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.DAL.Specifications
{
    public class LandingPageSpecification : BaseSpecification<LandingPage>
    {
        public LandingPageSpecification()
        {
            AddInclude(page => page.Sections);
            AddInclude(page => page.Product);
            ApplyOrderByDescending(page => page.UpdatedAt);
        }

        public LandingPageSpecification(int id)
            : base(page => page.Id == id)
        {
            AddInclude(page => page.Sections);
            AddInclude(page => page.Product);
        }

        public LandingPageSpecification(string slug)
            : base(page => page.Slug == slug)
        {
            AddInclude(page => page.Sections);
        }

        public LandingPageSpecification(string slug, int excludeId)
            : base(page => page.Slug == slug && page.Id != excludeId)
        {
        }

        public static LandingPageSpecification ForProduct(int productId)
        {
            return new LandingPageSpecification(page => page.ProductId == productId);
        }

        private LandingPageSpecification(Expression<Func<LandingPage, bool>> criteria)
            : base(criteria)
        {
            AddInclude(page => page.Sections);
            AddInclude(page => page.Product);
        }
    }
}
