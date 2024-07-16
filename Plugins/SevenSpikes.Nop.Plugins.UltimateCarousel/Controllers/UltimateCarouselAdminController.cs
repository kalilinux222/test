using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.ActionAttributes;
using SevenSpikes.Nop.Framework.Controllers;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Mappings;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Controllers
{
    [AdminAuthorize]
    public class UltimateCarouselAdminController : Base7SpikesAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICarouselService _carouselService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly WidgetSettings _widgetSettings;

        public UltimateCarouselAdminController(ISettingService settingService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IPictureService pictureService,
            ICarouselService carouselService,
            ICacheManager cacheManager,
            IWorkContext workContext,
            WidgetSettings widgetSettings,
            IStoreService storeService,
            IStoreMappingService storeMappingService)
            : base(storeService, storeMappingService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _workContext = workContext;
            _pictureService = pictureService;
            _widgetSettings = widgetSettings;
            _storeService = storeService;
            _carouselService = carouselService;
            _cacheManager = cacheManager;
        }

        [SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(bool returnPartialView = false)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var ultimateCarouselSettings = _settingService.LoadSetting<UltimateCarouselSettings>(storeScope);

            var model = ultimateCarouselSettings.ToModel();

            model.EnabledAsWidget = _widgetSettings.ActiveWidgetSystemNames.Contains(Constants.PluginSystemName);

            // Store Settings
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                var storeScopeSetting = new StoreScopeSettingsHelper<UltimateCarouselSettings>(ultimateCarouselSettings, storeScope, _settingService);

                model.Enabled_OverrideForStore = storeScopeSetting.SettingExists(x => x.Enabled);
            }

            model.IsTrialVersion = false;

#if TRIAL
            model.IsTrialVersion = true;
#endif

            if (returnPartialView)
            {
                return PartialView(Constants.ViewAdminSettings, model);
            }

            return View(Constants.ViewAdminSettings, AdminViewLocations.AdminLayoutViewPath, model);
        }

        [HttpPost, SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(UltimateCarouselSettingsModel model, bool returnPartialView = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Settings");
            }

            if (model.EnabledAsWidget && !_widgetSettings.ActiveWidgetSystemNames.Contains(Constants.PluginSystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Constants.PluginSystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
            else
            {
                if (!model.EnabledAsWidget && _widgetSettings.ActiveWidgetSystemNames.Contains(Constants.PluginSystemName))
                {
                    _widgetSettings.ActiveWidgetSystemNames.Remove(Constants.PluginSystemName);
                    _settingService.SaveSetting(_widgetSettings);
                }
            }

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = model.ToEntity();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            var storeScopeSetting = new StoreScopeSettingsHelper<UltimateCarouselSettings>(settings, storeScope, _settingService);

            storeScopeSetting.SaveStoreSetting(model.Enabled_OverrideForStore, x => x.Enabled);

            //now clear settings cache
            _settingService.ClearCache();
            _cacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (returnPartialView)
            {
                var settingsModel = settings.ToModel();
                return PartialView(Constants.ViewAdminSettings, settingsModel);
            }

            return RedirectToAction("Settings");
        }

        // Carousel
        public ActionResult List()
        {
            var isTrialVersion = false;

            #region Trial
#if TRIAL
            isTrialVersion = true;
#endif
            #endregion

            var model = new UltimateCarouselSettingsModel
            {
                IsTrialVersion = isTrialVersion
            };

            return View(Constants.ViewAdminCarouselList, model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            var gridModel = PrepareListModel();

            var grids = new DataSourceResult()
            {
                Data = gridModel,
                Total = gridModel.Count
            };

            return Json(grids);
        }

        private IList<CarouselModel> PrepareListModel()
        {
            var carousels = _carouselService.GetAllCarousels();

            var carouselModels = new List<CarouselModel>();

            foreach (var carousel in carousels)
            {
                var carouselModel = new CarouselModel
                {
                    Id = carousel.Id,
                    PublicTitle = carousel.PublicTitle,
                    IsEnabled = carousel.IsEnabled,
                    DisplayOrder = carousel.DisplayOrder
                };
                AddLocales(_languageService, carouselModel.Locales);

                carouselModels.Add(carouselModel);
            }

            return carouselModels;
        }

        private IList<SelectListItem> GetAvailableTemplates()
        {
            try
            {
                return _carouselService.GetAvailableTemplates().ToList();
            }
            catch (Exception e)
            {
                LogException(e);

                ErrorNotification(e.Message);

                return new List<SelectListItem>();
            }
        }

        public ActionResult Create()
        {
            #region Trial
#if TRIAL
            IList<UCarousel> carousels = _carouselService.GetAllCarousels();

            if (carousels.Count > 0)
            {
                ErrorNotification("The trial is limited to only 1 carousel.");

                return RedirectToAction("List");
            }
#endif
            #endregion
            
            var model = new CarouselModel
            {
                UltimateCarouselSettings = new UltimateCarouselSettingsModel
                {
                    IsTrialVersion = false
                },
                CarouselItemsTemplatesList = GetAvailableTemplates()
            };

#if TRIAL
            model.UltimateCarouselSettings.IsTrialVersion = true;
#endif

            AddLocales(_languageService, model.Locales);

            return View(Constants.ViewAdminCarouselCreate, model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CarouselModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var carousel = model.ToEntity();

                _carouselService.InsertCarousel(carousel);

                SuccessNotification(_localizationService.GetResource("SevenSpikes.UltimateCarousel.Admin.Carousel.Created"));
                return continueEditing ? RedirectToAction("Edit", new { id = carousel.Id }) : RedirectToAction("List");
            }
            else
            {
                ErrorNotification(_localizationService.GetResource("SevenSpikes.UltimateCarousel.Admin.Carousel.Error"));
            }

            return RedirectToAction("Create", model);
        }

        public ActionResult Edit(int id)
        {
            var carousel = _carouselService.GetCarouselById(id);

            if (carousel == null)
            {
                throw new ArgumentException("No carousel found with the specified id");
            }

            var model = carousel.ToModel();
            model.CarouselItemsTemplatesList = GetAvailableTemplates();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.PublicTitle = carousel.GetLocalized(x => x.PublicTitle, languageId, false, false);
            });

            PrepareStoresMappingModel(model.MappingToStores, carousel, false);

            model.UltimateCarouselSettings.IsTrialVersion = false;

#if TRIAL
            model.UltimateCarouselSettings.IsTrialVersion = true;
#endif

            return View(Constants.ViewAdminCarouselEdit, model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(CarouselModel model, bool continueEditing)
        {
            var carousel = _carouselService.GetCarouselById(model.Id);

            if (carousel == null)
            {
                throw new ArgumentException("No carousel found with the specified id");
            }

            if (ModelState.IsValid)
            {
                carousel = model.ToEntity(carousel);

                _carouselService.UpdateCarousel(carousel);

                SaveStoreMappings(carousel, model.MappingToStores);
                
                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedValue(carousel, x => x.PublicTitle, localized.PublicTitle, localized.LanguageId);
                }

                SuccessNotification(_localizationService.GetResource("SevenSpikes.UltimateCarousel.Admin.Carousel.Updated"));
            }
            else
            {
                ErrorNotification(_localizationService.GetResource("SevenSpikes.UltimateCarousel.Admin.Carousel.Error"));
            }

            return continueEditing ? RedirectToAction("Edit", new { id = carousel.Id }) : RedirectToAction("List");
        }

        public ActionResult DeleteCarousel(int id, DataSourceRequest command)
        {
            var carousel = _carouselService.GetCarouselById(id);

            if (carousel == null)
            {
                throw new ArgumentException("No carousel found with the specified id.");
            }

            // Delete all carousel items first, because they have foreign key to the carousel
            var carouselItems = _carouselService.GetAllCarouselItemsForCarousel(id);
            foreach (var carouselItem in carouselItems)
            {
                DeleteItem(carouselItem.Id);
            }
            
            // Delete the carousel
            _carouselService.DeleteCarousel(carousel);

            return List(command);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var carousel = _carouselService.GetCarouselById(id);

            if (carousel == null)
            {
                throw new ArgumentException("No carousel found with the specified id.");
            }

            // Delete all carousel items first, because they have foreign key to the carousel
            var carouselItems = _carouselService.GetAllCarouselItemsForCarousel(id);
            foreach (var carouselItem in carouselItems)
            {
                DeleteItem(carouselItem.Id);
            }

            // Delete the carousel.
            _carouselService.DeleteCarousel(carousel);

            SuccessNotification(_localizationService.GetResource("SevenSpikes.UltimateCarousel.Admin.Carousel.Deleted"));
            return RedirectToAction("List");
        }

        // Carousel Items
        private void UpdateLocales(CarouselItem carouselItem, CarouselItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(carouselItem, x => x.Title, localized.Title, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(carouselItem, x => x.Description, localized.Description, localized.LanguageId);
            }
        }

        private IList<CarouselItemModel> PrepareItemsListModel(int carouselId)
        {
            var items = _carouselService.GetAllCarouselItemsForCarousel(carouselId);

            var itemModels = new List<CarouselItemModel>();

            foreach (var item in items)
            {
                var itemModel = new CarouselItemModel
                {
                    Id = item.Id,
                    CarouselId = item.CarouselId,
                    Title = item.Title,
                    Url = item.Url,
                    OpenInNewWindow = item.OpenInNewWindow,
                    IsPictureVisible = item.IsPictureVisible,
                    PictureSrc = _pictureService.GetPictureUrl(item.PictureId, 400),
                    Visible = item.Visible,
                    DisplayOrder = item.DisplayOrder
                };

                itemModels.Add(itemModel);

                AddLocales(_languageService, itemModel.Locales);
            }

            return itemModels;
        }

        [HttpPost]
        public ActionResult CarouselItemList(int carouselId, DataSourceRequest command)
        {
            var gridModel = PrepareItemsListModel(carouselId);

            var grids = new DataSourceResult()
            {
                Data = gridModel,
                Total = gridModel.Count
            };

            return Json(grids, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CarouselItemCreate(int carouselId)
        {
            #region Trial
#if TRIAL
            IList<CarouselItem> carouseItems = _carouselService.GetAllCarouselItemsForCarousel(carouselId);

            if (carouseItems.Count > 1)
            {
                ErrorNotification("The trial is limited to only 2 items per carousel.");
                ViewBag.isTrialVersion = true;
            }
#endif
            #endregion

            var model = new CarouselItemModel
            {
                CarouselId = carouselId
            };

            AddLocales(_languageService, model.Locales);

            return View(Constants.ViewAdminCarouselItemCreate, model);
        }

        [HttpPost]
        public ActionResult CarouselItemCreate(CarouselItemModel model)
        {
            if (ModelState.IsValid)
            {
                var carouselItem = new CarouselItem
                {
                    CarouselId = model.CarouselId,
                    Title = model.Title,
                    Description = model.Description,
                    Url = model.Url,
                    OpenInNewWindow = model.OpenInNewWindow,
                    IsPictureVisible = model.IsPictureVisible,
                    PictureId = model.PictureId,
                    Visible = model.Visible,
                    DisplayOrder = model.DisplayOrder
                };

                _carouselService.InsertCarouselItem(carouselItem);

                model.PictureSrc = _pictureService.GetPictureUrl(carouselItem.PictureId, 400);

                UpdateLocales(carouselItem, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = Request.Form["refreshButtonId"];
            }

            return View(Constants.ViewAdminCarouselItemCreate, model);
        }

        public ActionResult CarouselItemUpdate(int id)
        {
            var item = _carouselService.GetCarouselItemById(id);
            if (item == null)
            {
                throw new ArgumentException("No carousel item found with the specified id");
            }

            var model = new CarouselItemModel
            {
                CarouselId = item.CarouselId,
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Url = item.Url,
                OpenInNewWindow = item.OpenInNewWindow,
                IsPictureVisible = item.IsPictureVisible,
                PictureId = item.PictureId,
                PictureSrc = _pictureService.GetPictureUrl(item.PictureId, 400),
                Visible = item.Visible,
                DisplayOrder = item.DisplayOrder
            };

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = item.GetLocalized(x => x.Title, languageId, false, false);
                locale.Description = item.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(Constants.ViewAdminCarouselItemCreate, model);
        }

        [HttpPost]
        public ActionResult CarouselItemUpdate(CarouselItemModel model, DataSourceRequest command)
        {
            var carouselItem = _carouselService.GetCarouselItemById(model.Id);
            if (carouselItem == null)
            {
                throw new ArgumentException("No carousel item found with the specified id");
            }

            carouselItem.CarouselId = model.CarouselId;
            carouselItem.Id = model.Id;
            carouselItem.Title = model.Title;
            carouselItem.Description = model.Description;
            carouselItem.Url = model.Url;
            carouselItem.OpenInNewWindow = model.OpenInNewWindow;
            carouselItem.IsPictureVisible = model.IsPictureVisible;
            carouselItem.PictureId = model.PictureId;
            carouselItem.Visible = model.Visible;
            carouselItem.DisplayOrder = model.DisplayOrder;
            
            _carouselService.UpdateCarouselItem(carouselItem);

            UpdateLocales(carouselItem, model);
            
            ViewBag.RefreshPage = true;
            ViewBag.btnId = Request.Form["refreshButtonId"];

            return View(Constants.ViewAdminCarouselItemCreate, model);
        }

        public ActionResult CarouselItemDelete(int id, DataSourceRequest command)
        {
            DeleteItem(id);

            return List(command);
        }

        [NonAction]
        private void DeleteItem(int id)
        {
            var item = _carouselService.GetCarouselItemById(id);

            if (item == null)
            {
                throw new ArgumentException("No carousel item found with the specified id.");
            }

            _carouselService.DeleteCarouselItem(item);
        }

    }
}