using Nop.Web.Framework.Mvc;
using Nop.Web.Models.ShoppingCart;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.ActionFilters
{
    public class MiniShoppingCartActionFilterAttribute : BaseShoppingCartActionFilterAttribute
    {
        public override BaseNopModel GetModel(ActionExecutedContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;

            if (model is MiniShoppingCartModel miniShoppingCartModel)
            {
                return miniShoppingCartModel;
            }

            return null;
        }
    }
}