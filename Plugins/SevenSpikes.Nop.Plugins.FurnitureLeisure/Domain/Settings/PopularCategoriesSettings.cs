using Nop.Core.Configuration;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings
{
    public class PopularCategoriesSettings : ISettings
    {
        public PopularCategoriesSettings()
        {
            Categories = new List<int>();
        }

        public List<int> Categories { get; set; }
    }
}