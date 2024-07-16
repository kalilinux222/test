using Nop.Admin.Extensions;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using SevenSpikes.Nop.Plugins.JsonLD.Models.Admin;

namespace SevenSpikes.Nop.Plugins.JsonLD.ModelMapping
{
    public static class MappingExtensions
    {
        public static JsonLdAdminModel ToModel(this JsonLdSettings settings)
        {
            return settings.MapTo<JsonLdSettings, JsonLdAdminModel>();
        }

        public static JsonLdSettings ToEntity(this JsonLdAdminModel model)
        {
            return model.MapTo<JsonLdAdminModel, JsonLdSettings>();
        }
    }

}
