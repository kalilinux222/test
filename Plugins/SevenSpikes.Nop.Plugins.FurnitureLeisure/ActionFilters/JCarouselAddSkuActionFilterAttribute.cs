using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Framework.Mvc;
using SevenSpikes.Nop.Framework.ActionResults;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.ActionFilters
{
    public class JCarouselAddSkuActionFilterAttribute : ActionFilterAttribute
    {
        private IProductService _productService;

        private IProductService ProductService => _productService ?? (_productService = EngineContext.Current.Resolve<IProductService>());

        private ISettingService _settingService;

        private ISettingService SettingService => _settingService ?? (_settingService = EngineContext.Current.Resolve<ISettingService>());

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            bool showSku = SettingService.GetSettingByKey("jcarouselgeneralsettings.showsku", false);

            if (!showSku)
            {
                return;
            }

            if (!(filterContext.Result is AggregateActionResult7Spikes aggregateActionResult))
                return;

            var aggregationActionResultFields = typeof(AggregateActionResult7Spikes).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            if (aggregationActionResultFields.Length == 0)
                return;

            if (!(aggregationActionResultFields[0].GetValue(aggregateActionResult) is List<ActionResult> actionResults))
                return;

            foreach (var actionResult in actionResults)
            {
                if (!(actionResult is ViewResult viewResult))
                    continue;

                var model = viewResult.Model as dynamic;

                if (model == null)
                    continue;

                foreach (BaseNopEntityModel item in model.Items)
                {
                    var product = ProductService.GetProductById(item.Id);

                    if (product == null)
                        continue;

                    if (!item.CustomProperties.ContainsKey("SKU"))
                    {
                        item.CustomProperties.Add("SKU", product.Sku);
                    }
                }
            }
        }
    }
}