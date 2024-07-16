using CsvHelper;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers
{
	public class FurnitureLeisureImportManager : IFurnitureLeisureImportManager
	{
		private readonly IProductTagService _productTagService;
		private readonly IProductService _productService;

		public FurnitureLeisureImportManager(IProductTagService productTagService,
			IProductService productService)
		{
			_productTagService = productTagService;
			_productService = productService;
		}

		public void ImportCsv(Stream stream)
		{
			var models = GetModels(stream);

			AddProductTags(models);
		}

		public void ImportCsvAll(Stream stream)
		{
			var models = GetModels(stream);

			/*
			 * We delete all tags because we will completely change every tag in the database
			 * and all the tags mapped to products.
			 */
			var tags = _productTagService.GetAllProductTags();
			foreach(var tag in tags)
			{
				_productTagService.DeleteProductTag(tag);
			}

			AddProductTags(models);
		}

		private IList<ProductToProductTagsRelationModel> GetModels(Stream stream)
		{
			var models = new List<ProductToProductTagsRelationModel>();

			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				models = csv.GetRecords<ProductToProductTagsRelationModel>().ToList();
			}

			return models;
		}

		private void AddProductTags(IList<ProductToProductTagsRelationModel> models)
		{
			foreach (var model in models)
			{
				var product = _productService.GetProductById(model.ProductId);

				var productTags = model.ProductTags.Split(';').ToList();

				var productTagIdsToRemove = product.ProductTags
					.Where(x => !productTags.Contains(x.Name))
					.Select(x => x.Id)
					.ToList();

				foreach(var id in productTagIdsToRemove)
				{
					var tagToRemove = product.ProductTags.FirstOrDefault(x => x.Id == id);

					if(tagToRemove == null)
					{
						continue;
					}

					product.ProductTags.Remove(tagToRemove);
				}

				foreach (var tag in productTags)
				{
					if(string.IsNullOrEmpty(tag))
					{
						continue;
					}

					var productTag = _productTagService.GetProductTagByName(tag);

					if (productTag == null)
					{
						productTag = new ProductTag
						{
							Name = tag
						};

						_productTagService.InsertProductTag(productTag);
					}

					product.ProductTags.Add(productTag);
					_productService.UpdateProduct(product);
				}
			}
		}
	}
}
