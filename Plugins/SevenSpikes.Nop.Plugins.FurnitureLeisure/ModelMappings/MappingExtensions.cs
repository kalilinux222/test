using Nop.Admin.Extensions;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.ModelMappings
{
    public static class MappingExtensions
    {
        public static FurnitureLeisureSettingsModel ToModel(this FurnitureLeisureSettings settings)
        {
            return settings.MapTo<FurnitureLeisureSettings, FurnitureLeisureSettingsModel>();
        }

        public static FurnitureLeisureSettings ToEntity(this FurnitureLeisureSettingsModel model)
        {
            return model.MapTo<FurnitureLeisureSettingsModel, FurnitureLeisureSettings>();
        }
    }
}
