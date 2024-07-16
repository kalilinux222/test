using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public interface IDictionaryHelper
    {
        void CheckAddFieldToDictionary(IDictionary<string, object> dictionary, string key, string value);

        void CheckAddDictionaryToScripts(IList<string> scripts, IDictionary<string, object> dictionary);

        string GetStoreUrl(int storeId = 0, bool useSsl = false);

        void CheckUpdateDictionaryValue(IDictionary<string, object> dictionary, string key, object value);
    }
}
