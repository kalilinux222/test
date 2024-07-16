using Nop.Core.Configuration;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings
{
    public class FurnitureLeisureSettings : ISettings
    {
        public FurnitureLeisureSettings()
        {
            CarouselImagePairs = new List<string>();
        }

        public List<string> CarouselImagePairs { get; set; }

        public string WidgetZone { get; set; }

        public bool EnableCatalogRequestBanner { get; set; }

        public string LeftText { get; set; }

        public string Title { get; set; }

        public int CatalogRequestPageTopicId { get; set; }
    }
}