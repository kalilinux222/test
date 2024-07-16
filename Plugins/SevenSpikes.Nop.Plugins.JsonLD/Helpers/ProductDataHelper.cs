using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class ProductDataHelper : IProductDataHelper
    {
        private readonly JsonLdSettings _jsonLdSettings;
        private readonly IDictionaryHelper _dictionaryHelper;
        private readonly IWorkContext _workContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly SeoSettings _seoSettings;

        public ProductDataHelper(JsonLdSettings jsonLdSettings,
            IDictionaryHelper dictionaryHelper,
            IWorkContext workContext,
            IUrlRecordService urlRecordService,
            SeoSettings seoSettings)
        {
            _jsonLdSettings = jsonLdSettings;
            _dictionaryHelper = dictionaryHelper;
            _workContext = workContext;
            _urlRecordService = urlRecordService;
            _seoSettings = seoSettings;
        }
               
        public IList<Dictionary<string, object>> PrepareProductData(IList<ProductOverviewModel> products)
        {
            var productData = new List<Dictionary<string, object>>();

            foreach(var product in products)
            {
                var productUrl = string.Format("{0}{1}", _dictionaryHelper.GetStoreUrl(), product.SeName);
                var mpn = product.CustomProperties.ContainsKey("Mpn") ?
                    product.CustomProperties["Mpn"].ToString() : string.Empty;
                string gtin = product.CustomProperties.ContainsKey("Gtin") ?
                    product.CustomProperties["Gtin"].ToString() : string.Empty;
                var sku = product.CustomProperties.ContainsKey("Sku") ?
                    product.CustomProperties["Sku"].ToString() : string.Empty;

                var productId = string.IsNullOrEmpty(gtin) ?
                    string.IsNullOrEmpty(sku) ? product.Id.ToString() : sku :
                    gtin;
                
                var pictureUrl = product.DefaultPictureModel.ImageUrl;
                var currency = _workContext.WorkingCurrency.CurrencyCode;
                var productPrice = product.ProductPrice.PriceValue.ToString();
                var availability = product.ProductPrice.DisableBuyButton ? "https://schema.org/OutOfStock" : "https://schema.org/InStock";
                
                var data = CreateProductDataDictionary(productUrl: productUrl, productId: productId,
                    productName: product.Name, productDescription: product.ShortDescription,
                    mpn: mpn, sku: sku, gtin: gtin, pictureUrl: pictureUrl, priceCurrency: currency,
                    productPrice: productPrice, offerAvailability: availability);

                productData.Add(data);
            }

            return productData;
        }

        public Dictionary<string, object> PrepareProductData(ProductDetailsModel product)
        {
            var productUrl = string.Format("{0}{1}", _dictionaryHelper.GetStoreUrl(), product.SeName);
            var mpn = product.CustomProperties.ContainsKey("Mpn") ?
                product.CustomProperties["Mpn"].ToString() : string.Empty;
            string gtin = product.CustomProperties.ContainsKey("Gtin") ?
                product.CustomProperties["Gtin"].ToString() : string.Empty;
            var sku = product.CustomProperties.ContainsKey("Sku") ?
                product.CustomProperties["Sku"].ToString() : string.Empty;

            var productId = string.IsNullOrEmpty(gtin) ?
                string.IsNullOrEmpty(sku) ? product.Id.ToString() : sku :
                gtin;

            var pictureUrl = product.DefaultPictureModel.ImageUrl;
            var currency = _workContext.WorkingCurrency.CurrencyCode;
            var productPrice = product.ProductPrice.PriceValue.ToString();
            var availability = product.AddToCart.DisableBuyButton ? "https://schema.org/OutOfStock" : "https://schema.org/InStock";

            var productName = string.IsNullOrEmpty(product.MetaTitle) ? product.Name : product.MetaTitle;

            var brandName = product.ProductManufacturers.Count > 0 ? product.ProductManufacturers.First().Name : string.Empty;

            var productData = CreateProductDataDictionary(productUrl: productUrl, productId: productId,
                productName: productName, productDescription: product.ShortDescription,
                mpn: mpn, sku: sku, gtin: gtin, pictureUrl: pictureUrl, priceCurrency: currency,
                productPrice: productPrice, offerAvailability: availability, brandName: brandName);

            return productData;
        }

        private Dictionary<string, object> CreateProductDataDictionary(string productUrl, string productId,
            string productName, string productDescription, string mpn, string sku, string gtin, string pictureUrl,
            string priceCurrency, string productPrice, string offerAvailability, string brandName = "")
        {
            var seller = new Dictionary<string, object>() {
                { "@type", "Organization" },
                { "name", _seoSettings.DefaultTitle } 
            };

            var offer = new Dictionary<string, object>()
            {
                { "@type", "Offer" },
                { "priceCurrency", priceCurrency },
                { "price", productPrice },
                { "priceValidUntil", DateTime.Now.AddDays(90).ToString("yyyy-MM-dd") },
                { "availability", offerAvailability },
                { "itemCondition", "http://schema.org/NewCondition" },
                { "url", productUrl },
                { "seller", seller }
            };

            _dictionaryHelper.CheckAddFieldToDictionary(offer, "mpn", mpn);

            var offers = new List<Dictionary<string, object>>();
            offers.Add(offer);

            var productData = new Dictionary<string, object>()
            {
                { "@context", "http://schema.org/" },
                { "@type", "Product" },
                { "url", productUrl },
                { "productId", productId },
                { "name", productName },
                { "description", productDescription },
                { "offers", offers}
            };


            var brand = new Dictionary<string, object>()
            {
                { "name", brandName }
            };

            productData.Add("brand", brand);


            _dictionaryHelper.CheckAddFieldToDictionary(productData, "mpn", mpn);
            _dictionaryHelper.CheckAddFieldToDictionary(productData, "sku", sku);
            _dictionaryHelper.CheckAddFieldToDictionary(productData, "gtin", gtin);
            _dictionaryHelper.CheckAddFieldToDictionary(productData, "image", pictureUrl);

            return productData;
        }   
    }
}
