using AutoMapper;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Models;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Mappings
{
    public static class MappingExtensions
    {
        #region Carousel Model

        public static CarouselModel ToModel(this UCarousel entity)
        {
            return Mapper.Map<UCarousel, CarouselModel>(entity);
        }
        public static UCarousel ToEntity(this CarouselModel model)
        {
            return Mapper.Map<CarouselModel, UCarousel>(model);
        }
        public static UCarousel ToEntity(this CarouselModel model, UCarousel destination)
        {
            return Mapper.Map(model, destination);
        }

        #endregion

        #region Carousel Item Model

        public static CarouselItemModel ToModel(this CarouselItem entity)
        {
            return Mapper.Map<CarouselItem, CarouselItemModel>(entity);
        }
        public static CarouselItem ToEntity(this CarouselItemModel model)
        {
            return Mapper.Map<CarouselItemModel, CarouselItem>(model);
        }
        public static CarouselItem ToEntity(this CarouselItemModel model, CarouselItem destination)
        {
            return Mapper.Map(model, destination);
        }

        #endregion

        #region Ultimate Carousel Settings

        public static UltimateCarouselSettingsModel ToModel(this UltimateCarouselSettings entity)
        {
            return Mapper.Map<UltimateCarouselSettings, UltimateCarouselSettingsModel>(entity);
        }
        public static UltimateCarouselSettings ToEntity(this UltimateCarouselSettingsModel model)
        {
            return Mapper.Map<UltimateCarouselSettingsModel, UltimateCarouselSettings>(model);
        }
        public static UltimateCarouselSettings ToEntity(this UltimateCarouselSettingsModel model, UltimateCarouselSettings destination)
        {
            return Mapper.Map(model, destination);
        }

        #endregion
    }
}