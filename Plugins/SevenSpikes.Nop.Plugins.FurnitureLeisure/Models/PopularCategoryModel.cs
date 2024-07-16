using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    public class PopularCategoryModel : BaseNopEntityModel
    {
        public PopularCategoryModel()
        {
            PopularCategoryIds = new List<string>();
        }

        public string CategoryName { get; set; }

        public string PopularCategories { get; set; }

        public List<string> PopularCategoryIds { get; set; }
    }
}