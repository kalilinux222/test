using Nop.Web.Framework.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    public class HomePageCategoryModel : BaseNopEntityModel
    {
        public string CategorySeName { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }
    }
}