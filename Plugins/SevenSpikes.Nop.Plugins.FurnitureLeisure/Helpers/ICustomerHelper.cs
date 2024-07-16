using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers
{
    public interface ICustomerHelper
    {
        CustomerCreationResult CreateFurnitureLeisureCustomer(CatalogRequestModel model);
    }
}
