using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers
{
    public interface IEmailHelper
    {
        void SendEmail(CatalogRequestModel model);
    }
}
