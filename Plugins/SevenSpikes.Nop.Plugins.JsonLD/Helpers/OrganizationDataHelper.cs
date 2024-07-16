using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.UI;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SevenSpikes.Nop.Plugins.JsonLD.Helpers
{
    public class OrganizationDataHelper : IOrganizationDataHelper
    {
        private readonly JsonLdSettings _jsonLdSettings;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly IDictionaryHelper _dictionaryHelper;
        private readonly IPageHeadBuilder _pageHeadBuilder;
        private readonly StoreInformationSettings _storeInformationSettings;

        public OrganizationDataHelper(JsonLdSettings jsonLdSettings,
            IStoreContext storeContext,
            IStoreService storeService,
            IPictureService pictureService,
            IDictionaryHelper dictionaryHelper,
            IPageHeadBuilder pageHeadBuilder,
            StoreInformationSettings storeInformationSettings)
        {
            _jsonLdSettings = jsonLdSettings;
            _storeContext = storeContext;
            _storeService = storeService;
            _pictureService = pictureService;
            _dictionaryHelper = dictionaryHelper;
            _pageHeadBuilder = pageHeadBuilder;
            _storeInformationSettings = storeInformationSettings;
        }

        public Dictionary<string, object> PrepareSchemaData()
        {
            var storeUrl = _storeContext.CurrentStore.SslEnabled ?
                _storeContext.CurrentStore.SecureUrl :
                _storeContext.CurrentStore.Url;

            var potentialActionTarget = storeUrl + "/search?q={query}";

            var potentialAction = new Dictionary<string, object>() {
                { "@type", "SearchAction" },
                { "target", potentialActionTarget },
                { "query-input", "required name=query" },
            };

            var schemaData = new Dictionary<string, object>()
            {
                { "@context", "https://schema.org" },
                { "@type", "Website" },
                { "url", storeUrl },
                { "potentialAction", potentialAction }
            };

            return schemaData;
        }

        public Dictionary<string, object> PrepareOrganizationData()
        {
            var storeUrl = _storeContext.CurrentStore.SslEnabled ?
               _storeContext.CurrentStore.SecureUrl :
               _storeContext.CurrentStore.Url;

            var name = _pageHeadBuilder.GenerateTitle(true);

            var pictureUrl = _jsonLdSettings.PictureId > 0 ?
                _pictureService.GetPictureUrl(_jsonLdSettings.PictureId) :
                string.Empty;

            var address = new Dictionary<string, object>()
            {
                { "@type", "PostalAddress" }
            };

            _dictionaryHelper.CheckAddFieldToDictionary(address, "streetAddress", _jsonLdSettings.StreetAddress);
            _dictionaryHelper.CheckAddFieldToDictionary(address, "addressLocality", _jsonLdSettings.AddressLocality);
            _dictionaryHelper.CheckAddFieldToDictionary(address, "postalCode", _jsonLdSettings.PostalCode);
            _dictionaryHelper.CheckAddFieldToDictionary(address, "addressCountry", _jsonLdSettings.Country);

            var logoPath = GetLogoPath();

            var organizationData = new Dictionary<string, object>()
            {
                { "@context", "https://schema.org" },
                { "@type", "Organization" },
                { "name", name },
                { "url", storeUrl },
                { "description", _jsonLdSettings.Description },
                { "logo", logoPath }
            };

            var sameAsLinksList = new List<string>();
            if (!string.IsNullOrEmpty(_storeInformationSettings.FacebookLink))
            {
                sameAsLinksList.Add(_storeInformationSettings.FacebookLink);
            }
            if (!string.IsNullOrEmpty(_storeInformationSettings.TwitterLink))
            {
                sameAsLinksList.Add(_storeInformationSettings.TwitterLink);
            }
            if (!string.IsNullOrEmpty(_storeInformationSettings.YoutubeLink))
            {
                sameAsLinksList.Add(_storeInformationSettings.YoutubeLink);
            }

            if (sameAsLinksList.Count > 0)
            {
                organizationData.Add("sameAs", sameAsLinksList);
            }

            _dictionaryHelper.CheckAddFieldToDictionary(organizationData, "image", pictureUrl);
            _dictionaryHelper.CheckAddFieldToDictionary(organizationData, "telephone", _jsonLdSettings.Phone);

            if (address.Keys.Count > 1)
            {
                organizationData.Add("address", address);
            }

            return organizationData;
        }

        private string GetLogoPath()
        {
            var activeThemeName = _storeInformationSettings.DefaultStoreTheme;
            
            var logoPath = (string.Format("~/Themes/{0}/Content/images/logo.png", activeThemeName));
            var alternativeLogoPath = VirtualPathUtility.ToAbsolute(string.Format("~/Themes/{0}/Content/img/logo.png", activeThemeName));

            return !File.Exists(logoPath) ? 
                File.Exists(alternativeLogoPath) ? alternativeLogoPath : string.Empty :
                logoPath;
        }
    }
}
