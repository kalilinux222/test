using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Cache
{
    public class UltimateCarouselModelCacheEventConsumer:
        // Carousel
        IConsumer<EntityInserted<UCarousel>>,
        IConsumer<EntityUpdated<UCarousel>>,
        IConsumer<EntityDeleted<UCarousel>>,

        // Carousel Item
        IConsumer<EntityInserted<CarouselItem>>,
        IConsumer<EntityUpdated<CarouselItem>>,
        IConsumer<EntityDeleted<CarouselItem>>
    {

        private ICacheManager CacheManager { get; set; }

        public UltimateCarouselModelCacheEventConsumer()
        {
            CacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        public void HandleEvent(EntityInserted<UCarousel> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
        public void HandleEvent(EntityUpdated<UCarousel> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
        public void HandleEvent(EntityDeleted<UCarousel> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
        public void HandleEvent(EntityInserted<CarouselItem> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
        public void HandleEvent(EntityUpdated<CarouselItem> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
        public void HandleEvent(EntityDeleted<CarouselItem> eventMessage)
        {
            CacheManager.RemoveByPattern(Constants.UltimateCarouselCacheKey);
        }
    }
}