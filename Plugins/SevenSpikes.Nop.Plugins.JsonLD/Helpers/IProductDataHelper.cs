using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface IProductDataHelper
    {
        IList<Dictionary<string, object>> PrepareProductData(IList<ProductOverviewModel> product);

        Dictionary<string, object> PrepareProductData(ProductDetailsModel product);
    }
}
