using SevenSpikes.Nop.Conditions.Domain;
using SevenSpikes.Nop.Framework.Domain.Enums;
using System.Collections.Generic;
namespace SevenSpikes.Nop.Plugins.UltimateCarousel
{
    public static class Constants
    {
        public const string PluginSystemName = "SevenSpikes.Nop.Plugins.UltimateCarousel";
        public const string PluginFolderName = "SevenSpikes.Nop.Plugins.UltimateCarousel";
        public const string PluginControllerName = "SevenSpikes.Nop.Plugins.UltimateCarousel.Controllers";
        public const string PluginName = "Nop Ultimate Carousel";
        public const string PluginResourceName = "SevenSpikes.UltimateCarousel";

        public static EntityType EntityType = EntityType.UltimateCarousel;

        public const string UltimateCarouselCacheKey = "nop.pres.7spikes.ultimate.carousel";

        public const string ViewAdminSettings = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.Settings";
        public const string ViewAdminCarouselList = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.List";
        public const string ViewAdminCarouselCreate = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.Create";
        public const string ViewAdminCarouselCreateOrUpdate = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin._CreateOrUpdate";
        public const string ViewAdminCarouselEdit = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.Edit";
        public const string ViewAdminCarouselSettings = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.CarouselSettings";
        public const string ViewAdminCarouselItems = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.CarouselItems";

        public const string ViewAdminCarouselItemCreate = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.CreateCarouselItem";
        public const string ViewAdminCarouselItemCreateOrUpdate = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin._CreateOrUpdateCarouselItem";
        public const string ViewAdminCarouselItemEdit = "SevenSpikes.Nop.Plugins.UltimateCarousel.Views.UltimateCarouselAdmin.EditItem";

        public const string PathToThemeTemplates = "~/Plugins/SevenSpikes.Nop.Plugins.UltimateCarousel/Themes/{0}/Views/UltimateCarousel/Templates";
        public const string PathToTemplates = "~/Plugins/SevenSpikes.Nop.Plugins.UltimateCarousel/Views/UltimateCarousel/Templates";

        public const string ViewPublicCarousel = "UltimateCarousel";

        public const string UltimateCarouselContextName = "7spikes_ultimate_carousel_object_context";

        public const string PluginUrlInStore = "http://www.nop-templates.com/ultimate-carousel-plugin-for-nopcommerce";

        // TODO: Decide which conditions will be available

        public static IDictionary<ConditionType, IList<object>> AvailableConditionTypes = new Dictionary<ConditionType, IList<object>>
        {
            {
                    ConditionType.Customer,
                    new List<object>()
                    {
                        CustomerConditionProperty.Default,
                        CustomerConditionProperty.Email,
                        CustomerConditionProperty.IsInRole,
                        CustomerConditionProperty.IsLoggedIn,
                        CustomerConditionProperty.IsNotInRole,
                        CustomerConditionProperty.UserName
                    }
            },
            {
                    ConditionType.Product,
                    new List<object>()
                    {
                        ProductConditionProperty.Category,
                        ProductConditionProperty.Default,
                        ProductConditionProperty.DiscountAmmount,
                        ProductConditionProperty.DiscountPercentage,
                        ProductConditionProperty.Manufacturer,
                        ProductConditionProperty.MinPrice,
                        ProductConditionProperty.PreOrder,
                        ProductConditionProperty.PriceDifferenceAmmount,
                        ProductConditionProperty.PriceDifferencePercentage,
                        ProductConditionProperty.ProductAgeHours,
                        ProductConditionProperty.Quantity
                    }
            },
            {
                    ConditionType.Manufacturer,
                    new List<object>()
                    {
                        ManufacturerConditionProperty.Default,
                        ManufacturerConditionProperty.Name
                    }
            },
            {
                    ConditionType.Category,
                    new List<object>()
                    {
                        CategoryConditionProperty.Default,
                        CategoryConditionProperty.Name
                    }
            },
            {
                    ConditionType.ProductSpecification,
                    new List<object>()
            }
        };

    }
}