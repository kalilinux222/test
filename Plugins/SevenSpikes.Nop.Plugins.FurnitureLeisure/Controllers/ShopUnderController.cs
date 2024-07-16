using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Extensions;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers
{
    public class ShopUnderController : BasePublicController
    {
        private readonly ICacheManager _cacheManager;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;


        private readonly ShopUnderSettings _shopUnderSettings;

        public ShopUnderController(
            ICategoryService categoryService,
            IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ShopUnderSettings shopUnderSettings, 
            ICurrencyService currencyService, 
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            ISpecificationAttributeService specificationAttributeService, 
            ITaxService taxService, 
            IWebHelper webHelper, 
            MediaSettings mediaSettings, 
            CatalogSettings catalogSettings)
        {
            _cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            _categoryService = categoryService;
            _productService = productService;
            _storeContext = storeContext;
            _workContext = workContext;
            _shopUnderSettings = shopUnderSettings;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _specificationAttributeService = specificationAttributeService;
            _taxService = taxService;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
        }

        public ActionResult ShopUnder()
        {
            if (!_shopUnderSettings.IsEnabled)
            {
                return new EmptyResult();
            }

            var category = _categoryService.GetCategoryById(_shopUnderSettings.CategoryId);

            if (category == null)
            {
                return new EmptyResult();
            }

            var model = new CategoryModel
            {
                SeName = category.GetSeName()
            };

            IPagedList<Product> featuredProducts = _productService.SearchProducts(
                categoryIds: new List<int> { _shopUnderSettings.CategoryId },
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: true,

                priceMax: _currencyService.ConvertToPrimaryStoreCurrency(_shopUnderSettings.ShopUnder, _workContext.WorkingCurrency),
                orderBy: ProductSortingEnum.PriceDesc,
                pageSize: 5);

            model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();

            model.CustomProperties.Add("ShopUnder", _shopUnderSettings.ShopUnder);
            model.CustomProperties.Add("TitleText", _shopUnderSettings.TitleText);
            model.CustomProperties.Add("ButtonText", _shopUnderSettings.ButtonText);

            return View(model);
        }

        [NonAction]
        private IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            return this.PrepareProductOverviewModels(_workContext,
                _storeContext, _categoryService, _productService, _specificationAttributeService,
                _priceCalculationService, _priceFormatter, _permissionService,
                _localizationService, _taxService, _currencyService,
                _pictureService, _webHelper, _cacheManager,
                _catalogSettings, _mediaSettings, products,
                preparePriceModel, preparePictureModel,
                productThumbPictureSize, prepareSpecificationAttributes,
                forceRedirectionAfterAddingToCart);
        }
    }
}