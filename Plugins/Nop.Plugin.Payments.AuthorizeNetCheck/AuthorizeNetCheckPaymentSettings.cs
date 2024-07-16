using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.AuthorizeNetCheck
{
    public class AuthorizeNetCheckPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string TransactionKey { get; set; }

        public string LoginId { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}
