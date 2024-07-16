using Nop.Core;
using Nop.Core.Domain.Localization;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Domain
{
    public class CarouselItem : BaseEntity, ILocalizedEntity
    {
        public bool Visible { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public bool OpenInNewWindow { get; set; }

        public bool IsPictureVisible { get; set; }

        public int PictureId { get; set; }

        public int CarouselId { get; set; }

        public virtual UCarousel Carousel { get; set; }

        public int DisplayOrder { get; set; }
    }
}