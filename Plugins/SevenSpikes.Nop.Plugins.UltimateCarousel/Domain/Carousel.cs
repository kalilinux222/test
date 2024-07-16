using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Domain
{
    public class UCarousel : BaseEntity, IStoreMappingSupported, ILocalizedEntity
    {
        public bool IsEnabled { get; set; }

        public string PublicTitle { get; set; }

        public string CarouselCssClass { get; set; }

        public string CarouselItemsTemplate { get; set; }

        public int PictureSize { get; set; }

        public int DisplayOrder { get; set; }

        // Owl Settings
        public bool SettingCenter { get; set; }

        public bool SettingLoop { get; set; }

        public int SettingMargin { get; set; }

        public bool SettingNav { get; set; }

        public string SettingResponsive { get; set; }

        public bool SettingAutoPlay { get; set; }

        public int SettingAutoplayTimeout { get; set; }

        public bool SettingAutoPlayHoverOnPause { get; set; }

        public string SettingAdvancedSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
    }
}