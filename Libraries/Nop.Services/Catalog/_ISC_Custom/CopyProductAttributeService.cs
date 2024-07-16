using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductAttributeService : ICopyProductAttributeService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public CopyProductAttributeService(IProductService productService,
            IProductAttributeService productAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService, 
            IPictureService pictureService,
            ICategoryService categoryService, 
            IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
            IDownloadService downloadService,
            IProductAttributeParser productAttributeParser,
            IUrlRecordService urlRecordService, 
            IStoreMappingService storeMappingService)
        {
            this._productService = productService;
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._pictureService = pictureService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._specificationAttributeService = specificationAttributeService;
            this._downloadService = downloadService;
            this._productAttributeParser = productAttributeParser;
            this._urlRecordService = urlRecordService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copy of product attribute data for a given attribute to all products that have the same attribute
        /// </summary>
        /// <param name="fromProduct">The product from which to copy a given attribute information to other products</param>
        /// <param name="fromAttrMap">Product attribute mapping to the other products</param>
        /// <returns>none</returns>
        public virtual void CopyProductAttributes(Product fromProduct, ProductAttributeMapping fromAttrMap)
        {
            if (fromProduct == null)
                throw new ArgumentNullException("fromProduct");

            if (fromAttrMap == null)
                throw new ArgumentNullException("fromAttrMap");

            //for localizations
            var languages = _languageService.GetAllLanguages(true);


            //get all attribute values we need to copy
            var fromAttrValues = fromAttrMap.ProductAttributeValues;

            //variable to store all 'from' product pictures associated with attribute values
            var originalPictureIdentifiers = new Dictionary<int, int>();
            foreach (var productPicture in fromProduct.ProductPictures)
            {
                    //var picture = productPicture.Picture;
                    //var pictureCopy = _pictureService.InsertPicture(
                    //    _pictureService.LoadPictureBinary(picture),
                    //    picture.MimeType,
                    //    _pictureService.GetPictureSeName(newName),
                    //    picture.AltAttribute,
                    //    picture.TitleAttribute);
                    //_productService.InsertProductPicture(new ProductPicture
                    //{
                    //    ProductId = productCopy.Id,
                    //    PictureId = pictureCopy.Id,
                    //    DisplayOrder = productPicture.DisplayOrder
                    //});
                foreach (var val in fromAttrValues){
                    if (val.PictureId == productPicture.Id){
                        originalPictureIdentifiers.Add(fromProduct.Id, productPicture.Id);
                    }
                }                    
            }


            //get all products that are using the attribute, whcih we would like to copy
            var toProducts = _productService.GetProductsByProductAtributeId(fromAttrMap.ProductAttributeId);
            if (toProducts != null)
            {
                foreach (var toProd in toProducts)
                {
                    //if this is not the product we are copying from
                    if (toProd.Id != fromProduct.Id)
                    {
                        //get a corresponding mapping
                        var toProdAttrMaps = _productAttributeService.GetProductAttributeMappingsByProductId(toProd.Id);
                        foreach (var toProdAttrMap in toProdAttrMaps)
                        {
                            //we only process matching attribute
                            if (toProdAttrMap.ProductAttributeId == fromAttrMap.ProductAttributeId)
                            {
                                //get all product attribute values
                                var toProdAttrValues = _productAttributeService.GetProductAttributeValues(toProdAttrMap.Id);
                                //delete all current values
                                foreach (var toProdAttrVal in toProdAttrValues)
                                {
                                    _productAttributeService.DeleteProductAttributeValue(toProdAttrVal);
                                }
                                //create new mappings
                                foreach (var fromProdAttrVal in fromAttrMap.ProductAttributeValues)
                                {
                                    int attributeValuePictureId = 0;
                                    //if (originalNewPictureIdentifiers.ContainsKey(fromProdAttrVal.PictureId))
                                    //{
                                    //    attributeValuePictureId = originalNewPictureIdentifiers[fromProdAttrVal.PictureId];
                                    //}
                                    var attributeValueCopy = new ProductAttributeValue
                                    {
                                        ProductAttributeMappingId = fromProdAttrVal.Id,
                                        AttributeValueTypeId = fromProdAttrVal.AttributeValueTypeId,
                                        AssociatedProductId = fromProdAttrVal.AssociatedProductId,
                                        Name = fromProdAttrVal.Name,
                                        ColorSquaresRgb = fromProdAttrVal.ColorSquaresRgb,
                                        PriceAdjustment = fromProdAttrVal.PriceAdjustment,
                                        WeightAdjustment = fromProdAttrVal.WeightAdjustment,
                                        Cost = fromProdAttrVal.Cost,
                                        Quantity = fromProdAttrVal.Quantity,
                                        IsPreSelected = fromProdAttrVal.IsPreSelected,
                                        DisplayOrder = fromProdAttrVal.DisplayOrder,
                                        PictureId = attributeValuePictureId,
                                    };
                                    _productAttributeService.InsertProductAttributeValue(attributeValueCopy);
                                    //localization
                                    foreach (var lang in languages)
                                    {
                                        var name = fromProdAttrVal.GetLocalized(x => x.Name, lang.Id, false, false);
                                        if (!String.IsNullOrEmpty(name))
                                            _localizedEntityService.SaveLocalizedValue(attributeValueCopy, x => x.Name, name, lang.Id);
                                    }
                                }
                            }
                        }
                        }
                    }
                }

        }

        /// <summary>
        /// Apply predefined values accross products that have the same attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute for which apply predefined values to all related products</param>
        /// <returns>none</returns>
        public virtual void ApplyPredefinedAttributeValues(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException("productAttribute");

            //for localizations
            var languages = _languageService.GetAllLanguages(true);

            //predefined values
            var predefinedValues = _productAttributeService.GetPredefinedProductAttributeValues(productAttribute.Id);

            //get all products that are using the attribute, whcih we would like to copy
            var toProducts = _productService.GetProductsByProductAtributeId(productAttribute.Id);
            if (toProducts != null)
            {
                foreach (var toProd in toProducts)
                {
                    //get a corresponding mapping
                    var toProdAttrMaps = _productAttributeService.GetProductAttributeMappingsByProductId(toProd.Id);
                    foreach (var toProdAttrMap in toProdAttrMaps)
                    {
                        //we only process matching attribute
                        if (toProdAttrMap.ProductAttributeId == productAttribute.Id)
                        {
                            //get all product attribute values
                            var toProdAttrValues = _productAttributeService.GetProductAttributeValues(toProdAttrMap.Id);
                            //delete all current values
                            foreach (var toProdAttrVal in toProdAttrValues)
                            {
                                _productAttributeService.DeleteProductAttributeValue(toProdAttrVal);
                            }
                            //create new mappings
                            foreach (var predefinedValue in predefinedValues)
                            {
                                int attributeValuePictureId = 0;
                                var pav = new ProductAttributeValue
                                {
                                    ProductAttributeMappingId = toProdAttrMap.Id,
                                    AttributeValueType = AttributeValueType.Simple,
                                    Name = predefinedValue.Name,
                                    PriceAdjustment = predefinedValue.PriceAdjustment,
                                    WeightAdjustment = predefinedValue.WeightAdjustment,
                                    Cost = predefinedValue.Cost,
                                    IsPreSelected = predefinedValue.IsPreSelected,
                                    DisplayOrder = predefinedValue.DisplayOrder,
                                    PictureId = attributeValuePictureId,
                                };
                                _productAttributeService.InsertProductAttributeValue(pav);
                                //localization
                                foreach (var lang in languages)
                                {
                                    var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
                                    if (!String.IsNullOrEmpty(name))
                                        _localizedEntityService.SaveLocalizedValue(pav, x => x.Name, name, lang.Id);
                                }
                            }

                        }
                    }
                }
            }

            
        }

        #endregion
    }
}
