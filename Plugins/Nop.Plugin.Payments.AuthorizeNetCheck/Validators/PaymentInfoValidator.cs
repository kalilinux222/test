using FluentValidation;
using Nop.Plugin.Payments.AuthorizeNetCheck.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.AuthorizeNetCheck.Validators
{
    public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.RoutingNumber).Length(1, 9).WithMessage(localizationService.GetResource("Payment.AuthorizeNetCheck.RoutingNumber.Invalid"));
            RuleFor(x => x.AccountNumber).Length(1, 17).WithMessage(localizationService.GetResource("Payment.AuthorizeNetCheck.AccountNumber.Invalid"));
            RuleFor(x => x.NameOnAccount).Length(1, 22).WithMessage(localizationService.GetResource("Payment.AuthorizeNetCheck.NameOnAccount.Invalid"));
            RuleFor(x => x.BankName).Length(0, 50).WithMessage(localizationService.GetResource("Payment.AuthorizeNetCheck.BankName.Invalid"));
        }
    }
}