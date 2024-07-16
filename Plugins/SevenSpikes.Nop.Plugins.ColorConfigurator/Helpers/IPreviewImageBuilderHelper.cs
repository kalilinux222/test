using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers
{
    public interface IPreviewImageBuilderHelper
    {
        string BuildImage(Product product, IList<ProductAttributeValue> attributeValues, int pictureSize = 0);
    }
}
