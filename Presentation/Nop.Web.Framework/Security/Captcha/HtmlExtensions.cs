using Nop.Core.Infrastructure;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Framework.Security.Captcha
{
    public static class HtmlExtensions
    {
        public static IHtmlString GenerateCaptcha(this HtmlHelper helper)
        {
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();

            var lang = "en";

            //generate captcha control
            var captchaControl = new GRecaptchaControl
            {
                Theme = captchaSettings.ReCaptchaTheme,
                Id = "recaptcha",
                PublicKey = captchaSettings.ReCaptchaPublicKey,
                Language = lang
            };

            return new HtmlString(captchaControl.RenderControl());
        }
    }
}
