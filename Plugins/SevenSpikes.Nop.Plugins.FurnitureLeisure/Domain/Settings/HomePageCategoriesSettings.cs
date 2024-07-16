using Nop.Core.Configuration;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings
{
    public class HomePageCategoriesSettings : ISettings
    {
        public HomePageCategoriesSettings()
        {
            Categories = new List<int>();
        }

        public List<int> Categories { get; set; }
    }
}