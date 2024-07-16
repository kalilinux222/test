using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using System;
using System.Web.Mvc;

namespace Nop.Web.Framework.Security.Captcha
{
    public class CaptchaValidatorAttribute : ActionFilterAttribute
    {
        #region Constants

        private const string CHALLENGE_FIELD_KEY = "recaptcha_challenge_field";
        private const string RESPONSE_FIELD_KEY = "recaptcha_response_field";
        private const string G_RESPONSE_FIELD_KEY = "g-recaptcha-response";

        #endregion

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();

            //whether CAPTCHA is enabled
            if (captchaSettings.Enabled && context.HttpContext?.Request != null)
            {
                //push the validation result as an action parameter
                context.ActionParameters["captchaValid"] = ValidateCaptcha(context, captchaSettings.ReCaptchaPrivateKey);
            }
            else
                context.ActionParameters["captchaValid"] = false;


            base.OnActionExecuting(context);
        }

        private bool ValidateCaptcha(ActionExecutingContext context, string reCaptchaPrivateKey)
        {
            var isValid = false;

            //get form values
            var captchaChallengeValue = context.HttpContext.Request.Form[CHALLENGE_FIELD_KEY];
            var captchaResponseValue = context.HttpContext.Request.Form[RESPONSE_FIELD_KEY];
            var gCaptchaResponseValue = context.HttpContext.Request.Form[G_RESPONSE_FIELD_KEY];

            if ((!string.IsNullOrEmpty(captchaChallengeValue) && !string.IsNullOrEmpty(captchaResponseValue)) || !string.IsNullOrEmpty(gCaptchaResponseValue))
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();

                //create CAPTCHA validator
                var captchaValidtor = new GReCaptchaValidator()
                {
                    SecretKey = reCaptchaPrivateKey,
                    RemoteIp = webHelper.GetCurrentIpAddress(),
                    Response = !string.IsNullOrEmpty(captchaResponseValue) ? captchaResponseValue : gCaptchaResponseValue,
                    Challenge = captchaChallengeValue
                };

                //validate request
                var recaptchaResponse = captchaValidtor.Validate();
                isValid = recaptchaResponse.IsValid;
            }

            return isValid;
        }
    }
}
