using System.Data.Entity.ModelConfiguration;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Mappings.Carousels
{
    public class CarouselItemMap : EntityTypeConfiguration<CarouselItem>
    {
        public CarouselItemMap()
        {
            ToTable("SS_UC_CarouselItem");

            HasKey(pt => pt.Id);

            Property(pt => pt.Visible);
            Property(pt => pt.Title).HasMaxLength(400);
            Property(pt => pt.Description).IsMaxLength();
            Property(pt => pt.Url).IsMaxLength();
            Property(pt => pt.PictureId).IsRequired();
            Property(pt => pt.IsPictureVisible);
            Property(pt => pt.PictureId).IsOptional();
            Property(pt => pt.CarouselId).IsRequired();
            HasRequired(si => si.Carousel).WithMany().HasForeignKey(si => si.CarouselId).WillCascadeOnDelete(true);
            Property(pt => pt.DisplayOrder);
        }
    }
}