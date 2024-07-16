using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers
{
    public interface IAttributeHelper
    {
        IList<ProductAttributeValue> GetAttributeValuesFromXml(string attributesXml);

        IList<ProductAttributeValue> GetAttributeValuesFromForm(Product product, FormCollection form);
    }
}
