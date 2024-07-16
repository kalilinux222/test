using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Consumers
{
    public class FeaturedProductsEventConsumer :
        IConsumer<EntityInserted<ProductCategory>>,
        IConsumer<EntityUpdated<ProductCategory>>,
        IConsumer<EntityDeleted<ProductCategory>>
    {
        private const string CATEGORY_FEATURED_PRODUCT_IDS_PATTERN_KEY = "Nop.pres.category.featuredproduct.ids";

        private readonly ICacheManager _cacheManager;

        public FeaturedProductsEventConsumer()
        {
            _cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        //product categories
        public void HandleEvent(EntityInserted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_FEATURED_PRODUCT_IDS_PATTERN_KEY);
        }

        public void HandleEvent(EntityUpdated<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_FEATURED_PRODUCT_IDS_PATTERN_KEY);
        }

        public void HandleEvent(EntityDeleted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_FEATURED_PRODUCT_IDS_PATTERN_KEY);
        }
    }
}