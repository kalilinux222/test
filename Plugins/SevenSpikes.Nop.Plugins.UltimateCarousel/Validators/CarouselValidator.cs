using FluentValidation;
using Nop.Services.Localization;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Validators
{
    public class CarouselValidator : AbstractValidator<CarouselModel>
    {
        public CarouselValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PublicTitle).NotNull().WithMessage(localizationService.GetResource("SevenSpikes.Plugins.UltimateCarousel.Fields.CarouselName.IsRequired"));
            RuleFor(x => x.CarouselItemsTemplate).NotNull().WithMessage(localizationService.GetResource("SevenSpikes.Plugins.UltimateCarousel.Fields.CarouselItemsTemplate.IsRequired"));
        }
    }
}
