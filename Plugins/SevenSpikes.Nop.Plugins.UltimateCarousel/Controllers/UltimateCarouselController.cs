using System;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework.ActionResults;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SevenSpikes.Nop.Framework.Theme;
using SevenSpikes.Nop.Conditions.Services;
using SevenSpikes.Nop.Conditions.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Catalog;
using SevenSpikes.Nop.Conditions.Domain.FakeEntity;
using SevenSpikes.Nop.Conditions.Helpers;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Controllers
{
    public class UltimateCarouselController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICarouselService _carouselService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private ICacheManager _staticCacheManager;
        private readonly IConditionService _conditionService;
        private readonly IConditionChecker _conditionChecker;
        private readonly UltimateCarouselSettings _ultimateCarouselSettings;
        private readonly WidgetSettings _widgetSettings;

        private ICacheManager StaticCacheManager
        {
            get
            {
                if (_staticCacheManager == null)
                {
                    _staticCacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
                }

                return _staticCacheManager;
            }
        }

        private const string UltimateCarouselModelKey = Constants.UltimateCarouselCacheKey + "-{0}-{1}-{2}-{3}-{4}-{5}-{6}";

        #region Trial
#if TRIAL
        private const string MaxNoneTrialRequests = "5";
        private static int _trialcounter;
        private readonly string _trialmessage = string.Format("You are running a Trial version of <a href='{0}'>{1}</a>", Constants.PluginUrlInStore, Constants.PluginName);
#endif
        #endregion

        public UltimateCarouselController(IWorkContext workContext,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            ICarouselService carouselService,
            IConditionService conditionService,
            IConditionChecker conditionChecker,
            WidgetSettings widgetSettings,
            UltimateCarouselSettings ultimateCarouselSettings, IProductService productService, IManufacturerService manufacturerService, ICategoryService categoryService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _ultimateCarouselSettings = ultimateCarouselSettings;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _conditionChecker = conditionChecker;
            _conditionService = conditionService;
            _carouselService = carouselService;
            _widgetSettings = widgetSettings;
        }

        public ActionResult UltimateCarousel(string widgetZone)
        {
            if (!PluginHelper.IsPluginInstalled(Constants.PluginSystemName))
                return Content(Constants.PluginName + " is not installed.");

            if (!_ultimateCarouselSettings.Enabled || !_widgetSettings.ActiveWidgetSystemNames.Contains(Constants.PluginSystemName))
            {
                return new EmptyResult();
            }
            
            var productId = 0;
            if (Url.RequestContext.RouteData.Values.Keys.Contains("productid"))
            {
                productId = Convert.ToInt32(Url.RequestContext.RouteData.Values["productid"].ToString());
            }

            var categoryId = 0;
            if (Url.RequestContext.RouteData.Values.Keys.Contains("categoryId"))
            {
                categoryId = Convert.ToInt32(Url.RequestContext.RouteData.Values["categoryId"].ToString());
            }

            var manufacturerId = 0;
            if (Url.RequestContext.RouteData.Values.Keys.Contains("manufacturerId"))
            {
                manufacturerId = Convert.ToInt32(Url.RequestContext.RouteData.Values["manufacturerId"].ToString());
            }

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
            var cacheKey = string.Format(UltimateCarouselModelKey, string.Join(",", customerRolesIds), _storeContext.CurrentStore.Id, _webHelper.IsCurrentConnectionSecured(), widgetZone, productId, categoryId, manufacturerId);
            var cacheModel = StaticCacheManager.Get(cacheKey, () => PrepareCachedCarouselsModels(widgetZone, productId, categoryId, manufacturerId));

            if (cacheModel.Count > 0)
            {
                var aggregateActionResult = new AggregateActionResult7Spikes();
                foreach (var carouselModel in cacheModel)
                {
                    carouselModel.Theme = ThemeHelper.GetPluginTheme(Constants.PluginFolderName);

                    var carouselWidgetResult = CreateView(Constants.ViewPublicCarousel, carouselModel);
                    aggregateActionResult.AddActionResult(carouselWidgetResult);
                }

                if (aggregateActionResult.ActionResultsCount > 0)
                {
                    #region Trial

#if TRIAL
                    if(ShouldReturnTrialMessage())
                    {
                    return Content(_trialmessage);
                    }
#endif

                    #endregion

                    return aggregateActionResult;
                }
            }

            return Content(String.Empty);
        }

        private IList<CarouselModel> PrepareCachedCarouselsModels(string widgetZone, int productId, int categoryId, int manufacturerId)
        {
            var carousels = _carouselService.GetVisibleCarouselsByWidgetZone(widgetZone).Where(x => _storeMappingService.Authorize(x)).ToList();

            Customer customer = _workContext.CurrentCustomer;

            var carouselModels = new List<CarouselModel>();
            foreach (var carousel in carousels)
            {
                if (!CheckCondition(carousel, customer, productId, categoryId, manufacturerId))
                {
                    continue;
                }

                var responsiveSetting = "{ 0: {items: 1}, 590: {items: 2}, 800: {items: 2} }";

                if (!string.IsNullOrWhiteSpace(carousel.SettingResponsive))
                {
                    responsiveSetting = "{ ";

                    var responsiveJsObject = carousel.SettingResponsive.Split(',');
                    foreach (var item in responsiveJsObject)
                    {
                        var splittedItem = item.Split(':');
                        var width = splittedItem[0].Trim();
                        var count = splittedItem[1].Trim();

                        responsiveSetting += width + ": {items: " + count + "}, ";
                    }

                    responsiveSetting = responsiveSetting.TrimEnd(',') + "}";
                }

                var carouselModel = new CarouselModel
                {
                    Id = carousel.Id,
                    PublicTitle = carousel.GetLocalized(x => x.PublicTitle),
                    CarouselCssClass = carousel.CarouselCssClass,
                    CarouselItemsTemplate = carousel.CarouselItemsTemplate,
                    CarouselItems = GetAllItemsForCarousel(carousel.Id, carousel.PictureSize),
                    SettingCenter = carousel.SettingCenter,
                    SettingLoop = carousel.SettingLoop,
                    SettingMargin = carousel.SettingMargin,
                    SettingNav = carousel.SettingNav,
                    SettingResponsive = responsiveSetting,
                    SettingAutoPlay = carousel.SettingAutoPlay,
                    SettingAutoplayTimeout = carousel.SettingAutoplayTimeout,
                    SettingAutoPlayHoverOnPause = carousel.SettingAutoPlayHoverOnPause,
                    SettingAdvancedSettings = carousel.SettingAdvancedSettings
                };

                carouselModels.Add(carouselModel);

                #region Trial
#if TRIAL
                break;
#endif
                #endregion
            }

            return carouselModels;
        }

        private ActionResult CreateView(string viewName, object model)
        {
            var view = View(viewName, model);
            view.ViewData = new ViewDataDictionary { Model = model };
            return view;
        }

        private IList<CarouselItemModel> GetAllItemsForCarousel(int carouselId, int pictureSize)
        {
            var allItems = _carouselService.GetAllVisibleCarouselItemsByCarouselId(carouselId);

            var allItemsModels = new List<CarouselItemModel>();

            foreach (var item in allItems)
            {
                if (item.Visible)
                {
                    var itemModel = new CarouselItemModel
                    {
                        Title = item.GetLocalized(x => x.Title),
                        Description = item.GetLocalized(x => x.Description),
                        Url = item.Url,
                        OpenInNewWindow = item.OpenInNewWindow,
                        IsPictureVisible = item.IsPictureVisible,
                        PictureId = item.PictureId,
                        PictureSrc = _pictureService.GetPictureUrl(item.PictureId, pictureSize)
                    };

                    // used just to create the image on the server which is used for retina displays.
                    var dummy = _pictureService.GetPictureUrl(item.PictureId);

                    if (!string.IsNullOrWhiteSpace(itemModel.Url) && itemModel.Url.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemModel.Url = "http://" + itemModel.Url;
                    }

                    allItemsModels.Add(itemModel);
                }
            }

            return allItemsModels.ToList();
        }

        private bool CheckCondition(UCarousel carousel, Customer customer, int productId, int categoryId, int manufacturerId)
        {
            var checkCondition = false;

            Condition condition = _conditionService.GetConditionByEntityTypeAndEntityId(Constants.EntityType, carousel.Id, true);

            if (condition != null)
            {
                var baseEntityList = new List<BaseEntity>() { customer };

                if (productId != 0)
                {
                    Product product = _productService.GetProductById(productId);
                    baseEntityList.Add(product);

                    //If we are on product page we should check productspecifications also as we dont know what condition types has been chosen(Product or ProductSpecification)
                    var productSpecification = new ProductSpecification(product);
                    baseEntityList.Add(productSpecification);
                }

                if (categoryId != 0)
                {
                    Category category = _categoryService.GetCategoryById(categoryId);
                    baseEntityList.Add(category);
                }

                if (manufacturerId != 0)
                {
                    Manufacturer manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
                    baseEntityList.Add(manufacturer);
                }

                checkCondition = _conditionChecker.Check(condition, baseEntityList);
            }

            return checkCondition;
        }

#if TRIAL
        private bool ShouldReturnTrialMessage()
        {
            _trialcounter++;
            if (_trialcounter >= int.Parse(MaxNoneTrialRequests))
            {
                _trialcounter = 0;

                return true;
            }

            return false;
        }
#endif
    }
}