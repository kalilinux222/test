using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using SevenSpikes.Nop.Framework.Model;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Validators;
using System.Collections.Generic;
using System.Web.Mvc;
namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Models
{
    [Validator(typeof(CarouselValidator))]
    public class CarouselModel : BaseNopEntityModel, ILocalizedModel<CarouselLocalizedModel>
    {
        public CarouselModel()
        {
            IsEnabled = true;
            CarouselItems = new List<CarouselItemModel>();
            Locales = new List<CarouselLocalizedModel>();
            MappingToStores = new StoreMappingModel();
            UltimateCarouselSettings = new UltimateCarouselSettingsModel();
            SettingCenter = true;
            SettingLoop = true;
            SettingMargin = 10;
            SettingNav = true;
            SettingResponsive = "0:1,590:2,800:2";
            SettingAutoPlay = false;
            SettingAutoplayTimeout = 5000;
            SettingAutoPlayHoverOnPause = true;
        }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.IsEnabled")]
        public bool IsEnabled { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.PublicTitle")]
        public string PublicTitle { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.CarouselCssClass")]
        public string CarouselCssClass { get; set; }

        public IList<SelectListItem> CarouselItemsTemplatesList { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.CarouselItemsTemplate")]
        public string CarouselItemsTemplate { get; set; }

        public IList<CarouselItemModel> CarouselItems { get; set; }
        
        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.PictureSize")]
        public int PictureSize { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string Theme { get; set; }

        public UltimateCarouselSettingsModel UltimateCarouselSettings { get; set; }

        // Owl Settings
        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingCenter")]
        public bool SettingCenter { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingLoop")]
        public bool SettingLoop { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingMargin")]
        public int SettingMargin { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingNav")]
        public bool SettingNav { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingResponsive")]
        public string SettingResponsive { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingAutoPlay")]
        public bool SettingAutoPlay { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingAutoplayTimeout")]
        public int SettingAutoplayTimeout { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingAutoPlayHoverOnPause")]
        public bool SettingAutoPlayHoverOnPause { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.Carousel.SettingAdvancedSettings")]
        public string SettingAdvancedSettings { get; set; }

        // Store mapping
        public StoreMappingModel MappingToStores { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }

        public IList<CarouselLocalizedModel> Locales { get; set; }
    }

    public class CarouselLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        public string PublicTitle { get; set; }
    }
}
