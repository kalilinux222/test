using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Models
{
    public class UltimateCarouselSettingsModel : ILocalizedModel<UltimateCarouselSettingsLocalizedModel>
    {
        public UltimateCarouselSettingsModel()
        {
            Locales = new List<UltimateCarouselSettingsLocalizedModel>();
        }

        public bool IsTrialVersion { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Settings.EnabledAsWidget")]
        public bool EnabledAsWidget { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Settings.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        public IList<UltimateCarouselSettingsLocalizedModel> Locales { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }

    public class UltimateCarouselSettingsLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }
    }
}
