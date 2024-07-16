using Nop.Admin.Models.Settings;
using Nop.Core;
using Nop.Plugin.Tax.Avalara.Models.Settings;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using System.Web.Mvc;

namespace Nop.Plugin.Tax.Avalara.Components
{
    public class TaxOriginController : BasePublicController
    {
        #region Fields

        private readonly AvalaraTaxSettings _avalaraTaxSettings;
        private readonly IPermissionService _permissionService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public TaxOriginController(AvalaraTaxSettings avalaraTaxSettings,
            IPermissionService permissionService,
            ITaxService taxService,
            IWorkContext workContext)
        {
            this._avalaraTaxSettings = avalaraTaxSettings;
            this._permissionService = permissionService;
            this._taxService = taxService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke the widget view component
        /// </summary>
        /// <param name="widgetZone">Widget zone</param>
        /// <param name="additionalData">Additional parameters</param>
        /// <returns>View component result</returns>
        public ActionResult TaxOrigin(string widgetZone, object additionalData)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content(string.Empty);

            //ensure that Avalara tax provider is active
            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals("admin_store_details_top"))
                return Content(string.Empty);

            //prepare model
            var model = new TaxOriginAddressTypeModel
            {
                AvalaraTaxOriginAddressType = (int)_avalaraTaxSettings.TaxOriginAddressType,
                TaxOriginAddressTypes = _avalaraTaxSettings.TaxOriginAddressType.ToSelectList()
            };
            var taxSettingsModel = new TaxSettingsModel();
            model.PrecedingElementId = nameof(taxSettingsModel.TaxBasedOn);

            return View("~/Plugins/Tax.Avalara/Views/Settings/TaxOriginAddressType.cshtml", model);
        }

        #endregion
    }
}