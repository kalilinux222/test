
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Copy product service
    /// </summary>
    public partial interface ICopyProductAttributeService
    {
        /// <summary>
        /// Copy of product attribute data for a given attribute to all products that have the same attribute
        /// </summary>
        /// <param name="fromProduct">The product from which to copy a given attribute information to other products</param>
        /// <param name="fromAttrMap">Product attribute mapping to the other products</param>
        /// <returns>none</returns>
        void CopyProductAttributes(Product fromProduct, ProductAttributeMapping fromAttrMap);

        /// <summary>
        /// Apply predefined values accross products that have the same attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute for which apply predefined values to all related products</param>
        /// <returns>none</returns>
        void ApplyPredefinedAttributeValues(ProductAttribute productAttribute);
    }
}
