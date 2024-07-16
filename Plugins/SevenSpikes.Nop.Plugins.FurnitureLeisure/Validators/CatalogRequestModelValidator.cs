using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Validators
{
    public class CatalogRequestModelValidator : AbstractValidator<CatalogRequestModel>
    {
        public CatalogRequestModelValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.FirstName.Error"));
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.LastName.Error"));
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.Email.Error"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.Email.Error"));
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.PhoneNumber.Error"));
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.CompanyName.Error"));
            RuleFor(x => x.StreetAddress)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.StreetAddress.Error"));
            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.City.Error"));
            RuleFor(x => x.StateId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.StateId.Error"));
            RuleFor(x => x.ZipCode)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.ZipCode.Error"));
            RuleFor(x => x.CreateAccount)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.IsUsefullInformation.Error"));
            RuleFor(x => x.ShouldRecieveCall)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.ShouldRecieveCall.Error"));
            RuleFor(x => x.Password)
                .NotEmpty()
                .When(x => x.CreateAccount == 2)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.Password.Error"));
            RuleFor(x => x.Password)
                .Length(customerSettings.PasswordMinLength, 999)
                .When(x => x.CreateAccount == 2)
                .WithMessage(string.Format(localizationService.GetResource("Account.Fields.Password.LengthValidation"), customerSettings.PasswordMinLength));
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .When(x => x.CreateAccount == 2)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.ConfirmPassword.Error.Empty"));
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .When(x => x.CreateAccount == 2)
                .WithMessage(localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.Fields.ConfirmPassword.Error.DontMatch"));
        }
    }
}
