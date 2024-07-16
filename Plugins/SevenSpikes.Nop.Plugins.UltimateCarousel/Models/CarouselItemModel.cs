using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Models
{
    [Validator(typeof(CarouselItemValidator))]
    public class CarouselItemModel : BaseNopEntityModel, ILocalizedModel<CarouselItemLocalizedModel>
    {
        public CarouselItemModel()
        {
            Visible = true;
            IsPictureVisible = false;
            Locales = new List<CarouselItemLocalizedModel>();
        }

        public int CarouselId { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Visible")]
        public bool Visible { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.OpenInNewWindow")]
        public bool OpenInNewWindow { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.IsPictureVisible")]
        public bool IsPictureVisible { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        public string PictureSrc { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string Theme { get; set; }

        public IList<CarouselItemLocalizedModel> Locales { get; set; }
    }

    public class CarouselItemLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("SevenSpikes.UltimateCarousel.Admin.CarouselItem.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }
}