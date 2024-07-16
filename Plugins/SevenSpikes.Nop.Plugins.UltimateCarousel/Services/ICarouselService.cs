using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Services
{
    public interface ICarouselService
    {
        /* ======= Carousel ======= */

        IList<UCarousel> GetAllCarousels();

        IList<UCarousel> GetVisibleCarouselsByWidgetZone(string widgetZone);

        UCarousel GetCarouselById(int carouselId);

        void InsertCarousel(UCarousel carousel);

        void UpdateCarousel(UCarousel carousel);

        void DeleteCarousel(UCarousel carousel);

        IList<SelectListItem> GetAvailableTemplates();

        /* ======= Carousel Items ======= */

        IList<CarouselItem> GetAllCarouselItemsForCarousel(int carouselId);

        IList<CarouselItem> GetAllVisibleCarouselItemsByCarouselId(int carouselId);

        CarouselItem GetCarouselItemById(int carouselItemId);

        void InsertCarouselItem(CarouselItem carouselItem);

        void UpdateCarouselItem(CarouselItem carouselItem);

        void DeleteCarouselItem(CarouselItem carouselItem);
    }
}