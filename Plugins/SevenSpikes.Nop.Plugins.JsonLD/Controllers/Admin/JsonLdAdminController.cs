using Nop.Admin.Controllers;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Stores;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.ActionAttributes;
using SevenSpikes.Nop.Framework.Controllers;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.JsonLD.ModelMapping;
using SevenSpikes.Nop.Plugins.JsonLD.Models.Admin;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.Controllers.Admin
{
    public class JsonLdAdminController : BaseAdminController
    {
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public JsonLdAdminController(IStoreService storeService,
            IWorkContext workContext,
            ISettingService settingService,
            WidgetSettings widgetSettings,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
            _widgetSettings = widgetSettings;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        [SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(bool returnPartialView = false)
        {
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);

            JsonLdSettings settings = _settingService.LoadSetting<JsonLdSettings>(storeScope);

            var model = settings.ToModel();

            model.Enable = model.Enable && _widgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName);

            if(storeScope > 0)
            {
                var storeScopeSettings = new StoreScopeSettingsHelper<JsonLdSettings>(settings, storeScope, _settingService);

                model.Enable_OverrideForStore = storeScopeSettings.SettingExists(x => x.Enable);
                model.Name_OverrideForStore = storeScopeSettings.SettingExists(x => x.Name);
                model.PictureId_OverrideForStore = storeScopeSettings.SettingExists(x => x.PictureId);
                model.StreetAddress_OverrideForStore = storeScopeSettings.SettingExists(x => x.StreetAddress);
                model.AddressLocality_OverrideForStore = storeScopeSettings.SettingExists(x => x.AddressLocality);
                model.PostalCode_OverrideForStore = storeScopeSettings.SettingExists(x => x.PostalCode);
                model.Country_OverrideForStore = storeScopeSettings.SettingExists(x => x.Country);
                model.Phone_OverrideForStore = storeScopeSettings.SettingExists(x => x.Phone);
            }

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return View("Settings", AdminViewLocations.AdminLayoutViewPath, model);
        }

        [HttpPost, SetReturnPartialViewParameterIfCalledFromWidgetsPage]
        public ActionResult Settings(JsonLdAdminModel model, bool returnPartialView = false)
        {
            if (!ModelState.IsValid)
            {
                RedirectToAction("Settings");
            }

            if(model.Enable && !_widgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Plugin.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
            
            //load settings for a chosen store scope
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = model.ToEntity();

            var storeScopeSettings = new StoreScopeSettingsHelper<JsonLdSettings>(settings, storeScope, _settingService);
            
            storeScopeSettings.SaveStoreSetting(model.Enable_OverrideForStore, x => x.Enable);
            storeScopeSettings.SaveStoreSetting(model.Name_OverrideForStore, x => x.Name);
            storeScopeSettings.SaveStoreSetting(model.PictureId_OverrideForStore, x => x.PictureId);
            storeScopeSettings.SaveStoreSetting(model.StreetAddress_OverrideForStore, x => x.StreetAddress);
            storeScopeSettings.SaveStoreSetting(model.AddressLocality_OverrideForStore, x => x.AddressLocality);
            storeScopeSettings.SaveStoreSetting(model.PostalCode_OverrideForStore, x => x.PostalCode);
            storeScopeSettings.SaveStoreSetting(model.Country_OverrideForStore, x => x.Country);
            storeScopeSettings.SaveStoreSetting(model.Phone_OverrideForStore, x => x.Phone);

            //now clear settings cache
            _settingService.ClearCache();

            _customerActivityService.InsertActivity("EditNopJsonLdSettings", "Edit Nop Json-ld settings");

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (returnPartialView)
            {
                return PartialView(model);
            }

            return RedirectToAction("Settings");
        }

    }
}
