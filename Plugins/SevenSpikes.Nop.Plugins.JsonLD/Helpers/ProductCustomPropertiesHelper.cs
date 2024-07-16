using Nop.Services.Catalog;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class ProductCustomPropertiesHelper : IProductCustomPropertiesHelper
    {
        private readonly IProductService _productService;

        public ProductCustomPropertiesHelper(IProductService productService)
        {
            _productService = productService;
        }

        public void AddCustomProperties(ProductDetailsModel model)
        {
            var product = _productService.GetProductById(model.Id);

            if (product != null)
            {
                CheckAddProperty("Gtin", product.FormatGtin(), model.CustomProperties);
                CheckAddProperty("Sku", product.FormatSku(), model.CustomProperties);
                CheckAddProperty("Mpn", product.FormatMpn(), model.CustomProperties);
            }
        }

        public void AddCustomProperties(IList<ProductOverviewModel> models)
        {
            var products = _productService.GetProductsByIds(models.Select(x => x.Id).ToArray());

            foreach(var model in models)
            {
                var product = products.FirstOrDefault(x => x.Id == model.Id);

                if (product != null)
                {
                    CheckAddProperty("Gtin", product.FormatGtin(), model.CustomProperties);
                    CheckAddProperty("Sku", product.FormatSku(), model.CustomProperties);
                    CheckAddProperty("Mpn", product.FormatMpn(), model.CustomProperties);
                }
            }
        }

        private void CheckAddProperty(string key, object value, Dictionary<string,object> customProperties)
        {
            if(value != null)
            {
                customProperties.Add(key, value);
            }
        }
    }
}
