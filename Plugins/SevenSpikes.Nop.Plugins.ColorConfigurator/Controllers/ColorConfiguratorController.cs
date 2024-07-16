using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Web.Controllers;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Controllers
{
    public class ColorConfiguratorController : BasePublicController
    {
        private readonly IAttributeHelper _attributeHelper;
        private readonly IPreviewImageBuilderHelper _previewImageBuilderHelper;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        private readonly MediaSettings _mediaSettings;

        public ColorConfiguratorController(
            IAttributeHelper attributeHelper, 
            IPreviewImageBuilderHelper previewImageBuilderHelper,
            IProductService productService,
            IWorkContext workContext, 
            MediaSettings mediaSettings)
        {
            _attributeHelper = attributeHelper;
            _previewImageBuilderHelper = previewImageBuilderHelper;
            _productService = productService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
        }

        [HttpPost]
        public string GetImagePreview(int productId, FormCollection form)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
            {
                return string.Empty;
            }

            var attributeValues = _attributeHelper.GetAttributeValuesFromForm(product, form);

            var getImageSize = GetImageSize(form);

            var imagePreview = _previewImageBuilderHelper.BuildImage(product, attributeValues, getImageSize);

            if (!string.IsNullOrEmpty(imagePreview))
            {
                return imagePreview;
            }

            return string.Empty;
        }
        
        [HttpGet]
        public string GetImagePreview(int shoppingCartItemId, int pictureSize = 0)
        {
            var shoppingCartItem = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .FirstOrDefault(x => x.Id == shoppingCartItemId);

            if (shoppingCartItem == null)
            {
                return string.Empty;
            }

            var attributeValues = _attributeHelper.GetAttributeValuesFromXml(shoppingCartItem.AttributesXml);

            var imagePreview = _previewImageBuilderHelper.BuildImage(shoppingCartItem.Product, attributeValues, pictureSize);

            if (!string.IsNullOrEmpty(imagePreview))
            {
                return imagePreview;
            }

            return string.Empty;
        }

        private int GetImageSize(FormCollection form)
        {
            var formPictureSize = form["colorConfigurationPictureSize"];

            if (!string.IsNullOrEmpty(formPictureSize))
            {
                int.TryParse(formPictureSize, out int pictureSize);

                if (pictureSize > 0)
                {
                    return pictureSize;
                }
            }

            return _mediaSettings.ProductDetailsPictureSize;
        }
    }
}