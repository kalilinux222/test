using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Services
{
	public interface IProductSearchTermService
	{
		ProductSearchTerms GetByProductId(int productId);

		void Insert(ProductSearchTerms searchTerm);

		void Update(ProductSearchTerms searchTerm);

		void Delete(ProductSearchTerms searchTerm);
	}
}
