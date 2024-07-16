using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Web.Framework.Mvc;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.ActionFilters
{
    public abstract class BaseShoppingCartActionFilterAttribute : ActionFilterAttribute
    {
        private IAttributeHelper _attributeHelper;
        private IProductService _productService;
        private IProductTemplateService _productTemplateService;
        private IWorkContext _workContext;
        private IPreviewImageBuilderHelper _previewImageBuilderHelper;

        private MediaSettings _mediaSettings;

        private IAttributeHelper AttributeHelper => _attributeHelper ?? (_attributeHelper = EngineContext.Current.Resolve<IAttributeHelper>());

        private IProductService ProductService => _productService ?? (_productService = EngineContext.Current.Resolve<IProductService>());

        private IProductTemplateService ProductTemplateService => _productTemplateService ?? (_productTemplateService = EngineContext.Current.Resolve<IProductTemplateService>());

        private IWorkContext WorkContext => _workContext ?? (_workContext = EngineContext.Current.Resolve<IWorkContext>());

        private IPreviewImageBuilderHelper PreviewImageBuilderHelper => _previewImageBuilderHelper ?? (_previewImageBuilderHelper = EngineContext.Current.Resolve<IPreviewImageBuilderHelper>());

        private MediaSettings MediaSettings => _mediaSettings ?? (_mediaSettings = EngineContext.Current.Resolve<MediaSettings>());
        
        public abstract BaseNopModel GetModel(ActionExecutedContext filterContext);

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            dynamic model = GetModel(filterContext);

            if (model == null)
            {
                return;
            }

            foreach (var shoppingCartItemModel in model.Items)
            {
                var imagePath = GetPreviewImagePathForShoppingCartItem(shoppingCartItemModel.ProductId, shoppingCartItemModel.Id);

                if (!string.IsNullOrEmpty(imagePath))
                {
                    shoppingCartItemModel.Picture.ImageUrl = imagePath;
                    shoppingCartItemModel.Picture.FullSizeImageUrl = imagePath;
                }
            }

            filterContext.Controller.ViewData.Model = model;
        }

        private string GetPreviewImagePathForShoppingCartItem(int productId, int shoppingCartItemId)
        {
            var product = ProductService.GetProductById(productId);

            if (product == null)
            {
                return string.Empty;
            }

            var productTemplate = ProductTemplateService.GetProductTemplateById(product.ProductTemplateId);

            if (productTemplate == null)
            {
                return string.Empty;
            }

            if (!productTemplate.ViewPath.EndsWith(Plugin.ColorConfiguratorPattern, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            var shoppingCartItem = WorkContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .FirstOrDefault(x => x.Id == shoppingCartItemId);

            if (shoppingCartItem == null)
            {
                return string.Empty;
            }

            var selectedAttributeValues = AttributeHelper.GetAttributeValuesFromXml(shoppingCartItem.AttributesXml);

            var imagePath = PreviewImageBuilderHelper.BuildImage(product, selectedAttributeValues, MediaSettings.CartThumbPictureSize);

            return imagePath;
        }
    }
}