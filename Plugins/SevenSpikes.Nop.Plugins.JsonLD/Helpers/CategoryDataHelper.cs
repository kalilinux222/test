using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class CategoryDataHelper : ICategoryDataHelper
    {
        private readonly IDictionaryHelper _dictionaryHelper;

        public CategoryDataHelper(IDictionaryHelper dictionaryHelper)
        {
            _dictionaryHelper = dictionaryHelper;
        }

        public IDictionary<string, object> GenerateCategoryData(CategoryModel category)
        {
            var categoryUrl = GetEntityUrl(category.SeName);

            var categoryData = new Dictionary<string, object>()
            {
                { "@context", "http://schema.org" },
                { "@type", "ItemList" },
                { "name", category.Name },
                { "url", categoryUrl },
                { "description", category.Description },
                { "image", category.PictureModel.ImageUrl }
            };

            var mainEntityOfPageData = GenerateMainEntityOfPageData(category);

            if(mainEntityOfPageData.Keys.Count > 0)
            {
                categoryData.Add("mainEntityOfPage", mainEntityOfPageData);
            }

            var itemListData = GenerateItemListData(category.Products);

            if(itemListData.Count > 0)
            {
                categoryData.Add("itemListElement", itemListData);
            }

            return categoryData;
        }

        private IDictionary<string, object> GenerateMainEntityOfPageData(CategoryModel category)
        {
            var categoryUrl = GetEntityUrl(category.SeName);

            var mainEntiyOfPageData = new Dictionary<string, object>()
            {
                { "@type", "CollectionPage" },
                { "@id", categoryUrl },
            };

            return mainEntiyOfPageData;
        }

        private IList<IDictionary<string, object>> GenerateItemListData(IList<ProductOverviewModel> products)
        {
            var itemListData = new List<IDictionary<string, object>>();

            var count = 1;

            foreach(var product in products)
            {
                var productUrl = GetEntityUrl(product.SeName);

                var itemData = new Dictionary<string, object>()
                {
                    { "@type", "ListItem"},
                    { "position", count++},
                    { "url", productUrl}
                };

                itemListData.Add(itemData);
            }

            return itemListData;
        }

        private string GetEntityUrl(string seName)
        {
            var url = string.Format("{0}{1}", _dictionaryHelper.GetStoreUrl(), seName);

            return url;
        }
    }
}
