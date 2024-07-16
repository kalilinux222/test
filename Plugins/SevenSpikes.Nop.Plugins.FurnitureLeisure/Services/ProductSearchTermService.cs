using Nop.Core.Data;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Services
{
	public class ProductSearchTermService : IProductSearchTermService
	{
		private readonly IRepository<ProductSearchTerms> _productSearchTermsRepository;

		public ProductSearchTermService(IRepository<ProductSearchTerms> productSearchTermsRepository)
		{
			_productSearchTermsRepository = productSearchTermsRepository;
		}

		public ProductSearchTerms GetByProductId(int productId)
		{
			return _productSearchTermsRepository.Table.FirstOrDefault(x => x.ProductId == productId);
		}

		public void Insert(ProductSearchTerms searchTerm)
		{
			_productSearchTermsRepository.Insert(searchTerm);
		}

		public void Update(ProductSearchTerms searchTerm)
		{
			_productSearchTermsRepository.Update(searchTerm);
		}

		public void Delete(ProductSearchTerms searchTerm)
		{
			_productSearchTermsRepository.Delete(searchTerm);
		}
	}
}
