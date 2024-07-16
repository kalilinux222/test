using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.AuthorizeNetCheck.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetCheck.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetCheck.Fields.TransactionKey")]
        public string TransactionKey { get; set; }
        public bool TransactionKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetCheck.Fields.LoginId")]
        public string LoginId { get; set; }
        public bool LoginId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}