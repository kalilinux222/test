using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using System.Data.Entity.ModelConfiguration;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Data
{
	public class ProductSearchTermMap : EntityTypeConfiguration<ProductSearchTerms>
	{
		public ProductSearchTermMap()
		{
			ToTable("SS_FL_ProductSearchTerms");

			HasKey(x => x.Id);
			Property(x => x.ProductId).IsRequired();
			Property(x => x.SearchTerm).HasMaxLength(4000).IsRequired();
		}
	}
}
