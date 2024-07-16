using Nop.Core.Configuration;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings
{
    public class ShopUnderSettings : ISettings
    {
        public bool IsEnabled { get; set; }

        public int CategoryId { get; set; }

        public decimal ShopUnder { get; set; }

        public string TitleText { get; set; }

        public string ButtonText { get; set; }
    }
}