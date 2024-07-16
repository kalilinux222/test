using Nop.Admin.Controllers;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.ActionAttributes;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin
{
    public class ShopUnderAdminController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        private readonly ShopUnderSettings _shopUnderSettings;

        public ShopUnderAdminController(
            ICategoryService categoryService, 
            ILocalizationService localizationService, 
            ISettingService settingService, 
            IStoreService storeService, 
            IWorkContext workContext, 
            ShopUnderSettings shopUnderSettings)
        {
            _categoryService = categoryService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _workContext = workContext;

            _shopUnderSettings = shopUnderSettings;
        }

        public ActionResult Settings(bool returnPartialView = false)
        {
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);

            ShopUnderSettings settings = _settingService.LoadSetting<ShopUnderSettings>(storeScope);

            var model = new ShopUnderModel
            {
                IsEnabled = settings.IsEnabled,
                CategoryId = settings.CategoryId,
                ShopUnder = settings.ShopUnder,
                TitleText = settings.TitleText,
                ButtonText = settings.ButtonText
            };

            PrepareAllCategoriesModel(model);

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return View("Settings", AdminViewLocations.AdminLayoutViewPath, model);
        }

        [HttpPost, SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(ShopUnderModel model, bool returnPartialView = false)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("Settings");
            }

            _shopUnderSettings.IsEnabled = model.IsEnabled;
            _shopUnderSettings.CategoryId = model.CategoryId;
            _shopUnderSettings.ShopUnder = model.ShopUnder;
            _shopUnderSettings.TitleText = model.TitleText;
            _shopUnderSettings.ButtonText = model.ButtonText;

            _settingService.SaveSetting(_shopUnderSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return RedirectToAction("Settings");
        }


        [NonAction]
        private void PrepareAllCategoriesModel(ShopUnderModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableCategories.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });

            var categories = _categoryService.GetAllCategories(showHidden: true);

            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }
        }
    }
}
