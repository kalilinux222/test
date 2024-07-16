using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface IOrganizationDataHelper
    {
        Dictionary<string, object> PrepareSchemaData();

        Dictionary<string, object> PrepareOrganizationData();
    }
}
