using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class DictionaryHelper : IDictionaryHelper
    {
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        public DictionaryHelper(IStoreContext storeContext,
            IStoreService storeService)
        {
            _storeContext = storeContext;
            _storeService = storeService;
        }

        public void CheckAddFieldToDictionary(IDictionary<string, object> dictionary, string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value) || dictionary == null)
            {
                return;
            }

            dictionary.Add(key, value);
        }

        public void CheckAddDictionaryToScripts(IList<string> scripts, IDictionary<string, object> dictionary)
        {
            if (dictionary.Keys.Count > 0)
            {
                var schemaDataJson = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                scripts.Add(schemaDataJson);
            }
        }

        public void CheckUpdateDictionaryValue(IDictionary<string, object> dictionary, string key, object value)
        {
            if(dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public string GetStoreUrl(int storeId = 0, bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
            {
                throw new Exception("No store could be loaded");
            }

            return useSsl ? store.SecureUrl : store.Url;
        }
    }
}
