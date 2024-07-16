using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using System.Data.Entity.ModelConfiguration;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Mappings.Carousels
{
    public class CarouselMap : EntityTypeConfiguration<UCarousel>
    {
        public CarouselMap()
        {
            ToTable("SS_UC_Carousel");

            HasKey(pt => pt.Id);

            Property(pt => pt.IsEnabled);
            Property(pt => pt.PublicTitle).IsRequired().HasMaxLength(400);
            Property(pt => pt.CarouselCssClass).HasMaxLength(200);
            Property(pt => pt.CarouselItemsTemplate).IsRequired().HasMaxLength(400);
            Property(pt => pt.PictureSize);
            Property(pt => pt.DisplayOrder);
            Property(pt => pt.SettingCenter);
            Property(pt => pt.SettingLoop);
            Property(pt => pt.SettingMargin);
            Property(pt => pt.SettingNav);
            Property(pt => pt.SettingResponsive);
            Property(pt => pt.SettingAutoPlay);
            Property(pt => pt.SettingAutoplayTimeout);
            Property(pt => pt.SettingAutoPlayHoverOnPause);
            Property(pt => pt.SettingAdvancedSettings).IsMaxLength();
        }
    }
}
