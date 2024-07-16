using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.AuthorizeNetCheck.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Payment.AuthorizeNetCheck.RoutingNumber")]
        public string RoutingNumber { get; set; }

        [NopResourceDisplayName("Payment.AuthorizeNetCheck.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Payment.AuthorizeNetCheck.NameOnAccount")]
        public string NameOnAccount { get; set; }

        [NopResourceDisplayName("Payment.AuthorizeNetCheck.BankName")]
        public string BankName { get; set; }

        [NopResourceDisplayName("Payment.AuthorizeNetCheck.CheckViaEmail")]
        public bool CheckViaEmail { get; set; }
    }
}