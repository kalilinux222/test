using SevenSpikes.Nop.Framework.ActionFilters;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.ActionFilters
{
    public class JsonLdProductFilterFactory : IControllerActionFilterFactory
    {
        public string ControllerName
        {
            get { return "Product"; }
        }

        public string ActionName
        {
            get { return "ProductDetails"; }
        }

        public ActionFilterAttribute GetActionFilterAttribute()
        {
            return new JsonLdProductFilterAttribute();
        }
    }
}
