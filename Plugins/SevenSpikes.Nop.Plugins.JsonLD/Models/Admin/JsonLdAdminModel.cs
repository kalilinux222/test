using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;

namespace SevenSpikes.Nop.Plugins.JsonLD.Models.Admin
{
    public class JsonLdAdminModel
    {
        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.Enable")]
        public bool Enable { get; set; }
        public bool Enable_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.Name")]
        public string Name { get; set; }
        public bool Name_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.Plugins.AnywhereSliders.Admin.SliderImage.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        public bool PictureId_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.AdditionalPages")]
        public string AdditionalPages { get; set; }
        public bool AdditionalPages_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.StreetAddress")]
        public string StreetAddress { get; set; }
        public bool StreetAddress_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.AddressLocality")]
        public string AddressLocality { get; set; }
        public bool AddressLocality_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.PostalCode")]
        public string PostalCode { get; set; }
        public bool PostalCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.Country")]
        public string Country { get; set; }
        public bool Country_OverrideForStore { get; set; }

        [NopResourceDisplayName("SevenSpikes.JsonLd.Admin.Settings.Phone")]
        public string Phone { get; set; }
        public bool Phone_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
