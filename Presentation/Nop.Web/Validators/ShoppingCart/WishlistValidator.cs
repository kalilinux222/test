using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Validators.ShoppingCart
{
    public class WishlistValidator : BaseNopValidator<WishlistModel.RequestAQuoteModel>
    {
        public WishlistValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Email).EmailAddress().NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.DeliveryAddress).NotEmpty().WithMessage("Delivery address is required");
            RuleFor(x => x.DeliveryZip).NotEmpty().WithMessage("Zip Code is required");
        }
    }
}