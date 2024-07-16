using Nop.Web.Framework;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    public class FurnitureLeisureSettingsModel
    {
        public FurnitureLeisureSettingsModel()
        {

        }        

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.Settings.EnableCatalogRequestBanner")]
        public bool EnableCatalogRequestBanner { get; set; }
        public bool EnableCatalogRequestBanner_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.Settings.LeftText")]
        public string LeftText { get; set; }
        public bool LeftText_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.Settings.Title")]
        public string Title { get; set; }
        public bool Title_OverrideForStore { get; set; }

        public SelectList AvailableWidgetZones { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
