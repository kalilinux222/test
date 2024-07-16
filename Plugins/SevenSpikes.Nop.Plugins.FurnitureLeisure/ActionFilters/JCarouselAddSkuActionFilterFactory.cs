using SevenSpikes.Nop.Framework.ActionFilters;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.ActionFilters
{
    public class JCarouselAddSkuActionFilterFactory : IControllerActionFilterFactory
    {
        public JCarouselAddSkuActionFilterFactory(string controller, string action)
        {
            ControllerName = controller;
            ActionName = action;
        }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public ActionFilterAttribute GetActionFilterAttribute()
        {
            return new JCarouselAddSkuActionFilterAttribute();
        }
    }
}