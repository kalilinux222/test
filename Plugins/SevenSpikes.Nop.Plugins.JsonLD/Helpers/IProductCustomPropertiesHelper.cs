using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface IProductCustomPropertiesHelper
    {
        void AddCustomProperties(ProductDetailsModel model);

        void AddCustomProperties(IList<ProductOverviewModel> models);
    }
}
