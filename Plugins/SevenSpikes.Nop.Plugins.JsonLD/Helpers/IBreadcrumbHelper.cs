using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using static Nop.Web.Models.Catalog.ProductDetailsModel;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface IBreadcrumbHelper
    {
        IDictionary<string, object> GetCategoryBreadcrumbData(IList<CategoryModel> categoryBreadcrumb);

        IDictionary<string, object> GetProductBreadcrumbData(ProductBreadcrumbModel productBreadcrumb);
    }
}
