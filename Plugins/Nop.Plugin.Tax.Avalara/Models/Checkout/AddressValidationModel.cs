using Nop.Web.Models.Common;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Tax.Avalara.Models.Checkout
{
    /// <summary>
    /// Represents an address validation model
    /// </summary>
    public class AddressValidationModel : BaseNopModel
    {
        public string Message { get; set; }

        public bool IsError { get; set; }

        public AddressModel Address { get; set; }

        public bool IsNewAddress { get; set; }

        public int AddressId { get; set; }

        public bool Address1Changed { get; set; }

        public bool StateProvinceNameChanged { get; set; }

        public bool CityChanged { get; set; }

        public bool ZipPostalCodeChanged { get; set; }

    }
}