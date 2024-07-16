using Nop.Core.Configuration;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Domain
{
    public class UltimateCarouselSettings : ISettings
    {
        public bool Enabled { get; set; }

        public bool EnabledAsWidget { get; set; }
    }
}