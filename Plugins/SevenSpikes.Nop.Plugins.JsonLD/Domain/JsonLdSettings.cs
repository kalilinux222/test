using Nop.Core.Configuration;

namespace SevenSpikes.Nop.Plugins.JsonLD.Domain
{
    public class JsonLdSettings : ISettings
    {
        public bool Enable { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int PictureId { get; set; }

        public string AdditionalPages { get; set; }

        public string StreetAddress { get; set; }

        public string AddressLocality { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }
            
    }
}
