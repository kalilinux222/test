using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface ICategoryDataHelper
    {
        IDictionary<string, object> GenerateCategoryData(CategoryModel category);
    }
}
