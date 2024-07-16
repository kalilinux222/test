using SevenSpikes.Nop.Framework.ActionFilters;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.ActionFilters
{
    public class MiniShoppingCartActionFilterFactory : IControllerActionFilterFactory
    {
        public MiniShoppingCartActionFilterFactory(string controller, string action)
        {
            ControllerName = controller;
            ActionName = action;
        }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public ActionFilterAttribute GetActionFilterAttribute()
        {
            return new MiniShoppingCartActionFilterAttribute();
        }
    }
}