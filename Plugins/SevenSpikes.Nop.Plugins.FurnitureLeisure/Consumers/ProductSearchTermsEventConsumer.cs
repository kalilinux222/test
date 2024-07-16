using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Services;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Consumers
{
	public class ProductSearchTermsEventConsumer :
		IConsumer<EntityInserted<Product>>,
		IConsumer<EntityUpdated<Product>>,
		IConsumer<EntityDeleted<Product>>
	{
		private IProductSearchTermService _productSearchTermService;

		private IProductSearchTermService ProductSearchTermService
		{
			get
			{
				if(_productSearchTermService == null)
				{
					_productSearchTermService = EngineContext.Current.Resolve<IProductSearchTermService>();
				}

				return _productSearchTermService;
			}
		}

		public void HandleEvent(EntityInserted<Product> eventMessage)
		{
			if(eventMessage.Entity == null)
			{
				return;
			}

			var searchTerm = GetSearchTermForProduct(eventMessage.Entity);

			var productSearchTerm = new ProductSearchTerms() { 
				ProductId = eventMessage.Entity.Id,
				SearchTerm = searchTerm
			};

			ProductSearchTermService.Insert(productSearchTerm);
		}

		public void HandleEvent(EntityUpdated<Product> eventMessage)
		{
			if (eventMessage.Entity == null)
			{
				return;
			}

			var searchTerm = GetSearchTermForProduct(eventMessage.Entity);


			var productSearchTerm = ProductSearchTermService.GetByProductId(eventMessage.Entity.Id);

			if (productSearchTerm == null)
			{
				productSearchTerm = new ProductSearchTerms()
				{
					ProductId = eventMessage.Entity.Id,
					SearchTerm = searchTerm
				};

				ProductSearchTermService.Insert(productSearchTerm);
			}
			else
			{
				productSearchTerm.SearchTerm = searchTerm;

				ProductSearchTermService.Update(productSearchTerm);
			}
		}

		public void HandleEvent(EntityDeleted<Product> eventMessage)
		{
			if (eventMessage.Entity == null)
			{
				return;
			}

			var searchTerm = ProductSearchTermService.GetByProductId(eventMessage.Entity.Id);

			if(searchTerm != null)
			{
				ProductSearchTermService.Delete(searchTerm);
			}
		}

		private string GetSearchTermForProduct(Product product)
		{
			string searchTerm = product.Name;

			foreach (var tag in product.ProductTags)
			{
				searchTerm += ' ' + tag.Name;
			}

			return searchTerm;
		}
	}
}
