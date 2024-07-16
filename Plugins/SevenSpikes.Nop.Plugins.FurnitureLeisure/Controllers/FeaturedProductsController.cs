using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Extensions;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers
{

    public class FeaturedProductsController : BasePublicController
    {
        private const string CATEGORY_FEATURED_PRODUCT_IDS_KEY = "Nop.pres.category.featuredproduct.ids-{0}-{1}-{2}";

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

        public FeaturedProductsController(
            ICategoryService categoryService, 
            IProductService productService, 
            IStoreContext storeContext, 
            IWorkContext workContext,
            
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

        public ActionResult FeaturedProducts(int categoryId)
        {
            var category = _categoryService.GetCategoryById(categoryId);

            if (category == null)
            {
                return new EmptyResult();
            }

            var model = new CategoryModel
            {
                SeName = category.GetSeName()
            };

            IList<Product> featuredProducts = new List<Product>();

            string cacheKey = string.Format(CATEGORY_FEATURED_PRODUCT_IDS_KEY, categoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);

            var featuredProductIdsCache = _cacheManager.Get<IList<int>>(cacheKey, () =>
            {
                featuredProducts = _productService.SearchProducts(
                        categoryIds: new List<int> { category.Id },
                        storeId: _storeContext.CurrentStore.Id,
                        visibleIndividuallyOnly: true,
                        featuredProducts: true);

                return featuredProducts.Select(p => p.Id).ToList();
            });

            if (featuredProductIdsCache.Any())
            {
                featuredProducts = _productService.GetProductsByIds(featuredProductIdsCache.ToArray());
            }

            if (featuredProducts.Any())
            {
                model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
            }

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