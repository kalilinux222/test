using SevenSpikes.Nop.Framework.ActionFilters;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.ActionFilters
{
    public class JsonLdCategoryFilterFactory : IControllerActionFilterFactory
    {
        public string ControllerName
        {
            get { return "Catalog"; }
        }

        public string ActionName
        {
            get { return "Category"; }
        }

        public ActionFilterAttribute GetActionFilterAttribute()
        {
            return new JsonLdCategoryFilterAttribute();
        }
    }
}
