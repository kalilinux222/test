using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers
{
    public class AttributeHelper : IAttributeHelper
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;

        public AttributeHelper(
            IProductAttributeService productAttributeService, 
            IProductAttributeParser productAttributeParser)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
        }

        public IList<ProductAttributeValue> GetAttributeValuesFromXml(string attributesXml)
        {
            IList<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();

            var attributeMappings = _productAttributeParser.ParseProductAttributeMappings(attributesXml);

            foreach (var attributeMapping in attributeMappings)
            {
                var stringValues = _productAttributeParser.ParseValues(attributesXml, attributeMapping.Id);

                foreach (var value in stringValues)
                {
                    if (int.TryParse(value, out int attributeValueId))
                    {
                        var attributeValue = _productAttributeService.GetProductAttributeValueById(attributeValueId);

                        if (attributeValue != null)
                        {
                            attributeValues.Add(attributeValue);
                        }
                    }
                }
            }

            return attributeValues;
        }

        public IList<ProductAttributeValue> GetAttributeValuesFromForm(Product product, FormCollection form)
        {
            IList<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);

            foreach (var attribute in productAttributes)
            {
                string controlId = $"product_attribute_{attribute.Id}";

                if (attribute.AttributeControlType == AttributeControlType.DropdownList)
                {
                    var formAttribute = form[controlId];

                    if (!string.IsNullOrEmpty(formAttribute))
                    {
                        int.TryParse(formAttribute, out int selectedAttributeId);

                        if (selectedAttributeId > 0)
                        {
                            ProductAttributeValue attributeValue = attribute.ProductAttributeValues.FirstOrDefault(x => x.Id == selectedAttributeId);

                            if (attributeValue != null)
                            {
                                attributeValues.Add(attributeValue);
                            }
                        }
                    }
                }
            }

            return attributeValues;
        }
    }
}