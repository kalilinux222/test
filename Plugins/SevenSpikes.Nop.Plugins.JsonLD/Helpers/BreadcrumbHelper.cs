using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using static Nop.Web.Models.Catalog.ProductDetailsModel;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class BreadcrumbHelper : IBreadcrumbHelper
    {
        private readonly IDictionaryHelper _dictionaryHelper;

        public BreadcrumbHelper(
            IDictionaryHelper dictionaryHelper)
        {
            _dictionaryHelper = dictionaryHelper;
        }

        public IDictionary<string, object> GetProductBreadcrumbData(ProductBreadcrumbModel productBreadcrumb)
        {
            if(productBreadcrumb == null)
            {
                return new Dictionary<string, object>();
            }
            
            var breadcrumbData = BuildInitialBreadcrumbData();

            IList<IDictionary<string, object>> items = new List<IDictionary<string, object>>();

            int count = 1;

            var homeItem = BuildCategoryBreadcrumbItem("Home", string.Empty, count++);

            items.Add(homeItem);

            foreach (var item in productBreadcrumb.CategoryBreadcrumb)
            {
                var breadcrumbItemData = BuildCategoryBreadcrumbItem(item.Name, item.SeName, count++);

                items.Add(breadcrumbItemData);
            }

            var productItem = BuildCategoryBreadcrumbItem(productBreadcrumb.ProductName, productBreadcrumb.ProductSeName, count);

            items.Add(productItem);

            breadcrumbData.Add("itemListElement", items);

            return breadcrumbData;
        }

        public IDictionary<string, object> GetCategoryBreadcrumbData(IList<CategoryModel> categoryBreadcrumb)
        {
            if(categoryBreadcrumb.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            var breadcrumbData = BuildInitialBreadcrumbData();

            IList<IDictionary<string, object>> items = new List<IDictionary<string, object>>();

            int count = 1;

            var homeItem = BuildCategoryBreadcrumbItem("Home", string.Empty, count++);

            items.Add(homeItem);

            foreach (var item in categoryBreadcrumb)
            {
                var breadcrumbItemData = BuildCategoryBreadcrumbItem(item.Name, item.SeName, count++);

                items.Add(breadcrumbItemData);
            }

            breadcrumbData.Add("itemListElement", items);

            return breadcrumbData;
        }

        private IDictionary<string, object> BuildInitialBreadcrumbData()
        {
            var breadCrumbData = new Dictionary<string, object>()
            {
                {"@context", "http://schema.org" },
                {"@type", "BreadcrumbList" }
            };

            return breadCrumbData;
        }

        private IDictionary<string, object> BuildCategoryBreadcrumbItem(string itemName, string itemSeName, int position)
        {
            var itemUrl = string.Format("{0}{1}", _dictionaryHelper.GetStoreUrl(), itemSeName);

            var itemData = new Dictionary<string, object>()
            {
                { "@type", "Website" },
                { "@id", itemUrl },
                { "name", itemName}
            };

            var breadcrumbItemData = new Dictionary<string, object>()
            {
                { "@type", "ListItem" },
                { "position", position },
                { "item", itemData }
            };

            return breadcrumbItemData;
        }
    }
}
