using Nop.Admin.Controllers;
using Nop.Core.Domain.Media;
using Nop.Services.Media;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Security;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Services.Configuration;
using Nop.Core;
using Nop.Services.Stores;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.ModelMappings;
using SevenSpikes.Nop.Framework.Controllers;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.ActionAttributes;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin
{
    public class FurnitureLeisureAdminController : BaseAdminController
    {
        private readonly IConvertToDictionaryHelper _convertToDictionaryHelper;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly FurnitureLeisureSettings _furnitureLeisureSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IInstallHelper _installHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        public FurnitureLeisureAdminController(
            IConvertToDictionaryHelper convertToDictionaryHelper,
            IPictureService pictureService,
            ISettingService settingService, 
            FurnitureLeisureSettings furnitureLeisureSettings,
            IWorkContext workContext,
            IStoreService storeService,
            IInstallHelper installHelper,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService)
        {
            _convertToDictionaryHelper = convertToDictionaryHelper;
            _pictureService = pictureService;
            _settingService = settingService;
            _furnitureLeisureSettings = furnitureLeisureSettings;
            _workContext = workContext;
            _storeService = storeService;
            _installHelper = installHelper;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
        }

        public ActionResult List()
        {
            return View(Views.List, new CarouselImageModel());
        }

        public ActionResult Settings(bool returnPartialView = false)
        {
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);

            FurnitureLeisureSettings settings = _settingService.LoadSetting<FurnitureLeisureSettings>(storeScope);

            var model = settings.ToModel();

            if (storeScope > 0)
            {
                var storeScopeSettings = new StoreScopeSettingsHelper<FurnitureLeisureSettings>(settings, storeScope, _settingService);

                model.EnableCatalogRequestBanner_OverrideForStore = storeScopeSettings.SettingExists(x => x.EnableCatalogRequestBanner);
                model.LeftText_OverrideForStore = storeScopeSettings.SettingExists(x => x.LeftText);
                model.Title_OverrideForStore = storeScopeSettings.SettingExists(x => x.Title);
            }

            model.AvailableWidgetZones = new SelectList(_installHelper.GetSupportedWidgetZones(Plugin.FolderName));

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return View("Settings", AdminViewLocations.AdminLayoutViewPath, model);
        }

        [HttpPost, SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(FurnitureLeisureSettingsModel model, bool returnPartialView = false)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("Settings");
            }

            //load settings for a chosen store scope
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = model.ToEntity();

            var storeScopeSettings = new StoreScopeSettingsHelper<FurnitureLeisureSettings>(settings, storeScope, _settingService);


            storeScopeSettings.SaveStoreSetting(model.EnableCatalogRequestBanner_OverrideForStore, x => x.EnableCatalogRequestBanner);
            storeScopeSettings.SaveStoreSetting(model.LeftText_OverrideForStore, x => x.LeftText);
            storeScopeSettings.SaveStoreSetting(model.Title_OverrideForStore, x => x.Title);

            //now clear settings cache
            _settingService.ClearCache();

            _customerActivityService.InsertActivity("EditFurnitureLeisureSettings", "Edit Furniture Leisure plugin settings");

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        [AdminAntiForgery(true)]
        public ActionResult List(DataSourceRequest command)
        {
            IList<string> carouselImagePairs = _furnitureLeisureSettings.CarouselImagePairs;

            IDictionary<int, int> carouselImageDictionaryFromSemicolonSeparatedPairs = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPair(carouselImagePairs);

            IList<CarouselImageModel> carouselImageModels = carouselImageDictionaryFromSemicolonSeparatedPairs
                .Select(x =>
                {
                    Picture picture = _pictureService.GetPictureById(x.Key);

                    if (picture == null)
                    {
                        throw new Exception("Picture cannot be loaded");
                    }

                    var model = new CarouselImageModel
                    {
                        PictureId = x.Key,
                        PictureUrl = _pictureService.GetPictureUrl(picture),
                        DisplayOrder = carouselImageDictionaryFromSemicolonSeparatedPairs[x.Key]
                    };

                    return model;
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = carouselImageModels,
                Total = carouselImageModels.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult Update(DataSourceRequest command, int pictureId, int displayOrder)
        {
            if (pictureId == 0)
            {
                return new JsonResult();
            }

            IList<string> carouselImagePairs = _furnitureLeisureSettings.CarouselImagePairs;

            IDictionary<int, int> carouselImageDictionaryFromSemicolonSeparatedPairs = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPair(carouselImagePairs);

            if (carouselImageDictionaryFromSemicolonSeparatedPairs.ContainsKey(pictureId))
            {
                _furnitureLeisureSettings.CarouselImagePairs.Remove(string.Format("{0}:{1}", pictureId, carouselImageDictionaryFromSemicolonSeparatedPairs[pictureId]));

                _furnitureLeisureSettings.CarouselImagePairs.Add(string.Format("{0}:{1}", pictureId, displayOrder));

                _settingService.SaveSetting(_furnitureLeisureSettings);
            }

            return List(command);
        }

        [HttpPost]
        public ActionResult Delete(DataSourceRequest command, int pictureId)
        {
            if (pictureId == 0)
            {
                return new JsonResult();
            }

            IList<string> carouselImagePairs = _furnitureLeisureSettings.CarouselImagePairs;

            IDictionary<int, int> carouselImageDictionaryFromSemicolonSeparatedPairs = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPair(carouselImagePairs);

            if (carouselImageDictionaryFromSemicolonSeparatedPairs.ContainsKey(pictureId))
            {
                _furnitureLeisureSettings.CarouselImagePairs.Remove(string.Format("{0}:{1}", pictureId, carouselImageDictionaryFromSemicolonSeparatedPairs[pictureId]));

                _settingService.SaveSetting(_furnitureLeisureSettings);
            }

            return List(command);
        }

        [ValidateInput(false)]
        public ActionResult AddImage(int pictureId, int displayOrder)
        {
            if (pictureId == 0)
            {
                throw new ArgumentException();
            }

            Picture picture = _pictureService.GetPictureById(pictureId);

            if (picture == null)
            {
                throw new ArgumentException("No picture found with the specified id");
            }

            IList<string> carouselImagePairs = _furnitureLeisureSettings.CarouselImagePairs;

            IDictionary<int, int> carouselImageDictionaryFromSemicolonSeparatedPairs = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPair(carouselImagePairs);

            if (!carouselImageDictionaryFromSemicolonSeparatedPairs.ContainsKey(pictureId))
            {
                _furnitureLeisureSettings.CarouselImagePairs.Add(string.Format("{0}:{1}", pictureId, displayOrder));
                _settingService.SaveSetting(_furnitureLeisureSettings);
            }

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename);

            _pictureService.SetSeoFilename(pictureId, "HomePageCarouselImage");

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
    }
}