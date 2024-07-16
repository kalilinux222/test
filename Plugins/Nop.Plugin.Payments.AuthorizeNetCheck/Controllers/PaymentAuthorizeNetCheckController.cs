using Nop.Core;
using Nop.Plugin.Payments.AuthorizeNetCheck.Models;
using Nop.Plugin.Payments.AuthorizeNetCheck.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.AuthorizeNetCheck.Controllers
{
    public class PaymentAuthorizeNetCheckController : BasePaymentController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IThemeContext _themeContext;
        private readonly IWorkContext _workContext;
        private readonly HttpContextBase _httpContext;


        public PaymentAuthorizeNetCheckController(
            ILocalizationService localizationService, 
            ISettingService settingService, 
            IStoreService storeService, 
            IThemeContext themeContext,
            IWorkContext workContext,
            HttpContextBase httpContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _themeContext = themeContext;
            _workContext = workContext;
            _httpContext = httpContext;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var authorizeNetCheckPaymentSettings = _settingService.LoadSetting<AuthorizeNetCheckPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = authorizeNetCheckPaymentSettings.UseSandbox,
                TransactionKey = authorizeNetCheckPaymentSettings.TransactionKey,
                LoginId = authorizeNetCheckPaymentSettings.LoginId,
                AdditionalFee = authorizeNetCheckPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = authorizeNetCheckPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(authorizeNetCheckPaymentSettings, x => x.UseSandbox, storeScope);
                model.TransactionKey_OverrideForStore = _settingService.SettingExists(authorizeNetCheckPaymentSettings, x => x.TransactionKey, storeScope);
                model.LoginId_OverrideForStore = _settingService.SettingExists(authorizeNetCheckPaymentSettings, x => x.LoginId, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(authorizeNetCheckPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(authorizeNetCheckPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.AuthorizeNetCheck/Views/PaymentAuthorizeNetCheck/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var authorizeNetCheckPaymentSettings = _settingService.LoadSetting<AuthorizeNetCheckPaymentSettings>(storeScope);

            //save settings
            authorizeNetCheckPaymentSettings.UseSandbox = model.UseSandbox;
            authorizeNetCheckPaymentSettings.TransactionKey = model.TransactionKey;
            authorizeNetCheckPaymentSettings.LoginId = model.LoginId;
            authorizeNetCheckPaymentSettings.AdditionalFee = model.AdditionalFee;
            authorizeNetCheckPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.UseSandbox_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(authorizeNetCheckPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(authorizeNetCheckPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.TransactionKey_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(authorizeNetCheckPaymentSettings, x => x.TransactionKey, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(authorizeNetCheckPaymentSettings, x => x.TransactionKey, storeScope);

            if (model.LoginId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(authorizeNetCheckPaymentSettings, x => x.LoginId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(authorizeNetCheckPaymentSettings, x => x.LoginId, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(authorizeNetCheckPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(authorizeNetCheckPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(authorizeNetCheckPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(authorizeNetCheckPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var form = this.Request.Form;

            var checkViaEmail = false;

            if (!string.IsNullOrEmpty(form["CheckViaEmail"]))
            {
                bool.TryParse(form["CheckViaEmail"], out checkViaEmail);
            }

            // set postback values, if any
            var model = new PaymentInfoModel
            {
                RoutingNumber = form["RoutingNumber"],
                AccountNumber = form["AccountNumber"],
                NameOnAccount = form["NameOnAccount"],
                BankName = form["BankName"],
                CheckViaEmail = checkViaEmail
            };

            if (_httpContext.Session["PaymentMethodErrors"] is Dictionary<string, string> paymentMethodErrors)
            {
                foreach (KeyValuePair<string, string> paymentMethodError in paymentMethodErrors)
                {
                    ModelState.AddModelError(paymentMethodError.Key, paymentMethodError.Value);
                }

                // Clear the errors.
                _httpContext.Session["PaymentMethodErrors"] = null;
            }

            return View("~/Plugins/Payments.AuthorizeNetCheck/Views/PaymentAuthorizeNetCheck/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new Dictionary<string, string>();

            var paymentMethodCheckType = form["paymentmethodcheck"];

            if (!string.IsNullOrEmpty(paymentMethodCheckType))
            {
                var type = (CheckType) Enum.Parse(typeof(CheckType), paymentMethodCheckType);

                if (type == CheckType.ECheck)
                {
                    var checkViaEmail = false;

                    if (!string.IsNullOrEmpty(form["CheckViaEmail"]))
                    {
                        bool.TryParse(form["CheckViaEmail"], out checkViaEmail);
                    }

                    if (!checkViaEmail)
                    {
                        //validate
                        var validator = new PaymentInfoValidator(_localizationService);

                        var model = new PaymentInfoModel
                        {
                            RoutingNumber = form["RoutingNumber"],
                            AccountNumber = form["AccountNumber"],
                            NameOnAccount = form["NameOnAccount"],
                            BankName = form["BankName"]
                        };

                        var validationResult = validator.Validate(model);

                        if (!validationResult.IsValid)
                        {
                            foreach (var error in validationResult.Errors)
                            {
                                warnings.Add(error.PropertyName, error.ErrorMessage);
                            }

                            _httpContext.Session["PaymentMethodErrors"] = warnings;
                        }
                    }
                }
            }

            return warnings.Values.ToList();
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            if (form["paymentmethodcheck"] == CheckType.MailIn.ToString())
            {
                paymentInfo.CustomValues.Add("Payment Method Type", "Mail-In Check");
                return paymentInfo;
            }

            if (!string.IsNullOrEmpty(form["CheckViaEmail"]))
            {
                bool.TryParse(form["CheckViaEmail"], out bool checkViaEmail);

                if (checkViaEmail)
                {
                    paymentInfo.CustomValues.Add("Payment Method Type", "E-Check via Email");
                    return paymentInfo;
                }
            }

            paymentInfo.CustomValues.Add("Payment Method Type", "Online E-Check");
            paymentInfo.CustomValues.Add("Customer Name", form["NameOnAccount"]);
            paymentInfo.CustomValues.Add("Bank Name", form["BankName"]);
            paymentInfo.CustomValues.Add("Bank ABA Routing Number", form["RoutingNumber"]);
            paymentInfo.CustomValues.Add("Bank Account Number", form["AccountNumber"]);

            return paymentInfo;
        }

        public ActionResult GetDownloadForm()
        {
            return File($"~/Themes/{_themeContext.WorkingThemeName}/Content/pdf/E-CheckAuthorizationForm.pdf", "application/pdf", "E-Check Authorization Form.pdf");
        }
    }
}