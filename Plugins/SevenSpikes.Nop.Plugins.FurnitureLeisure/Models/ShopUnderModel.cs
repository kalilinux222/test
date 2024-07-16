using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    public class ShopUnderModel : BaseNopModel
    {
        public ShopUnderModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.ShopUnder.Settings.IsEnabled")]
        public bool IsEnabled { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.ShopUnder.Settings.CategoryId")]
        public int CategoryId { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.ShopUnder.Settings.ShopUnder")]
        public decimal ShopUnder { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.ShopUnder.Settings.TitleText")]
        public string TitleText { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Admin.ShopUnder.Settings.ButtonText")]
        public string ButtonText { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
    }
}
