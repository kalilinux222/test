using FluentValidation;
using Nop.Services.Localization;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Validators
{
    public class CarouselItemValidator : AbstractValidator<CarouselItemModel>
    {
        public CarouselItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotNull().WithMessage(localizationService.GetResource("SevenSpikes.Plugins.UltimateCarousel.Fields.Title.IsRequired"));
        }
    }
}