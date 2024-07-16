using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain
{
	public class ProductSearchTerms : BaseEntity
	{
		public int ProductId { get; set; }

		public string SearchTerm { get; set; }
	}
}
