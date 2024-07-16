using Nop.Web.Framework.Mvc;
using Nop.Web.Models.ShoppingCart;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.ActionFilters
{
    public class ShoppingCartActionFilterAttribute : BaseShoppingCartActionFilterAttribute
    {
        public override BaseNopModel GetModel(ActionExecutedContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;
            
            if (model is ShoppingCartModel shoppingCartModel)
            {
                return shoppingCartModel;
            }

            return null;
        }
    }
}