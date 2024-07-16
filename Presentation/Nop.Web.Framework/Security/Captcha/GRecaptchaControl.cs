using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Nop.Web.Framework.Security.Captcha
{
    /// <summary>
    /// Google reCAPTCHA control
    /// </summary>
    public class GRecaptchaControl
    {
        private const string RECAPTCHA_API_URL = "https://www.google.com/recaptcha/api.js?onload=onloadCallback&render=explicit";

        /// <summary>
        /// Identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// reCAPTCHA theme
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// reCAPTCHA public key
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// Language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Render control
        /// </summary>
        /// <returns></returns>
        public string RenderControl()
        {
            SetTheme();

            var sb = new StringBuilder();

            var scriptCallbackTag = new TagBuilder("script");

            scriptCallbackTag.Attributes.Add("type", "text/javascript");
            scriptCallbackTag.InnerHtml = $"var onloadCallback = function() {{grecaptcha.render('{Id}', {{'sitekey' : '{PublicKey}', 'theme' : '{Theme}' }});}};";

            sb.Append(scriptCallbackTag.ToString(TagRenderMode.Normal));

            var captchaTag = new TagBuilder("div");
            captchaTag.Attributes.Add("id", Id);

            sb.Append(captchaTag.ToString(TagRenderMode.Normal));

            var scriptLoadApiTag = new TagBuilder("script");
            scriptLoadApiTag.Attributes.Add("src", RECAPTCHA_API_URL + (string.IsNullOrEmpty(Language) ? "" : $"&hl={Language}"));
            scriptLoadApiTag.Attributes.Add("async", null);
            scriptLoadApiTag.Attributes.Add("defer", null);

            sb.Append(scriptLoadApiTag.ToString(TagRenderMode.Normal));

            return sb.ToString();
        }

        private void SetTheme()
        {
            if (Theme == null)
                Theme = "";
            Theme = Theme.ToLower();

            var themes = new[] { "white", "blackglass", "red", "clean", "light", "dark" };

            switch (Theme)
            {
                case "clean":
                case "red":
                case "white":
                    Theme = "light";
                    break;
                case "blackglass":
                    Theme = "dark";
                    break;
                default:
                    if (!themes.Contains(Theme))
                    {
                        Theme = "light";
                    }
                    break;
            }
        }
    }
}