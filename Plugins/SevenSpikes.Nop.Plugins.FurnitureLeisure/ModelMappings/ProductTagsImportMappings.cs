using Nop.Core.Domain.Catalog;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.ModelMappings
{
	public static class ProductTagsImportMappings
	{
		public static ProductToProductTagsRelationModel ToImportModel(this Product product)
		{
			var model = new ProductToProductTagsRelationModel();

			model.ProductId = product.Id;
			model.ProductName = product.Name;

			var productTags = string.Empty;
			bool firstTag = true;

			foreach(var tag in product.ProductTags)
			{
				if(firstTag)
				{
					firstTag = false;
				}
				else
				{
					productTags += ";";
				}

				productTags += tag.Name;
			}

			model.ProductTags = productTags;

			return model;
		}
	}
}
