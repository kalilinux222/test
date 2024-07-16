using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Services.Catalog;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using SevenSpikes.Nop.Plugins.JsonLD.Helpers;
using SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.Controllers
{
    public class JsonLdController : BasePublicController
    {
        private readonly IStoreContext _storeContext;
        private readonly JsonLdSettings _jsonLdSettings;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IProductDataHelper _productDataHelper;
        private readonly IDictionaryHelper _dictionaryHelper;
        private readonly WidgetSettings _widgetSettings;
        private readonly IOrganizationDataHelper _organizationDataHelper;
        private readonly IBreadcrumbHelper _breadcrumbHelper;
        private readonly ICategoryDataHelper _categoryDataHelper;

        public JsonLdController(IStoreContext storeContext,
            JsonLdSettings jsonLdSettings,
            IProductService productService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IProductDataHelper productDataHelper,
            IDictionaryHelper dictionaryHelper,
            WidgetSettings widgetSettings,
            IOrganizationDataHelper organizationDataHelper,
            IBreadcrumbHelper breadcrumbHelper,
            ICategoryDataHelper categoryDataHelper)
        {
            _storeContext = storeContext;
            _jsonLdSettings = jsonLdSettings;
            _productService = productService;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _productDataHelper = productDataHelper;
            _dictionaryHelper = dictionaryHelper;
            _widgetSettings = widgetSettings;
            _organizationDataHelper = organizationDataHelper;
            _breadcrumbHelper = breadcrumbHelper;
            _categoryDataHelper = categoryDataHelper;
        }

        public ActionResult WidgetJsonLd()
        {
            if (!_jsonLdSettings.Enable || !_widgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName))
            {
                return Content("");
            }

            var jsonLdScripts = new List<string>();
            
            var schemaData = _organizationDataHelper.PrepareSchemaData();

            var organizationData = _organizationDataHelper.PrepareOrganizationData();
            
            IDictionary<string, object> breadcrumbData = new Dictionary<string, object>();

            var productData = new Dictionary<string, object>();
            if (Url.RequestContext.RouteData.Values.TryGetValue("productId", out var productIdRouteValue))
            {
                if(int.TryParse(productIdRouteValue.ToString(), out var productId))
                {
                    if (productId > 0)
                    {
                        var product = _productService.GetProductById(productId);
                        
                        if (product != null)
                        {
                            var productKey = string.Format(Plugin.JSONLD_PRODUCT_CACHE_KEY,
                                productId, _workContext.CurrentCustomer.Id,
                                _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);

                            ProductDetailsModel productModel = null;

                            if (_cacheManager.IsSet(productKey))
                            {
                                productModel = _cacheManager.Get<ProductDetailsModel>(productKey);

                                productData = _productDataHelper.PrepareProductData(productModel);

                                _dictionaryHelper.CheckUpdateDictionaryValue(organizationData, "description", product.ShortDescription);          

                                if (productModel.Breadcrumb != null)
                                {
                                    breadcrumbData = _breadcrumbHelper.GetProductBreadcrumbData(productModel.Breadcrumb);
                                }
                            }
                        }
                    }
                }
            }

            IDictionary<string, object> categoryData = new Dictionary<string, object>();
            if (Url.RequestContext.RouteData.Values.TryGetValue("categoryId", out var categoryIdRouteValue))
            {
                if (int.TryParse(categoryIdRouteValue.ToString(), out var categoryId))
                {
                    if (categoryId > 0)
                    {
                        var categoryKey = string.Format(Plugin.JSONLD_CATEGORY_CACHE_KEY,
                            categoryId, _workContext.CurrentCustomer.Id,
                            _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);

                        CategoryModel category = null;

                        if (_cacheManager.IsSet(categoryKey))
                        {
                            category = _cacheManager.Get<CategoryModel>(categoryKey);

                            categoryData = _categoryDataHelper.GenerateCategoryData(category);

                            _dictionaryHelper.CheckUpdateDictionaryValue(organizationData, "description", category.Description);
                        }

                        if (category != null)
                        {
                            breadcrumbData = _breadcrumbHelper.GetCategoryBreadcrumbData(category.CategoryBreadcrumb);
                        }
                    }
                }
            }

            _dictionaryHelper.CheckAddDictionaryToScripts(jsonLdScripts, schemaData);
            _dictionaryHelper.CheckAddDictionaryToScripts(jsonLdScripts, organizationData);
            _dictionaryHelper.CheckAddDictionaryToScripts(jsonLdScripts, productData);
            _dictionaryHelper.CheckAddDictionaryToScripts(jsonLdScripts, categoryData);
            _dictionaryHelper.CheckAddDictionaryToScripts(jsonLdScripts, breadcrumbData);

            return View("JsonLd", jsonLdScripts);
        }
    }
}
