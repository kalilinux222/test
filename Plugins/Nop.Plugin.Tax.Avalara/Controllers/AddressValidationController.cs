using Avalara.AvaTax.RestClient;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.Avalara.Models.Checkout;
using Nop.Plugin.Tax.Avalara.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Extensions;
using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Nop.Plugin.Tax.Avalara.Controllers
{
    public class AddressValidationController : BasePublicController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly ILogger _logger;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;
        private readonly AvalaraTaxManager _avalaraTaxManager;
        private readonly AvalaraTaxSettings _avalaraTaxSettings;
        #endregion

        #region Ctor

        public AddressValidationController(IAddressService addressService,
            ICustomerService customerService,
            IWorkContext workContext,
            TaxSettings taxSettings, 
            ITaxService taxService, 
            AvalaraTaxManager avalaraTaxManager,
            AvalaraTaxSettings avalaraTaxSettings, 
            ILogger logger, 
            ICountryService countryService,
            ILocalizationService localizationService, 
            IStateProvinceService stateProvinceService, AddressSettings addressSettings)
        {
            _addressService = addressService;
            _customerService = customerService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _taxService = taxService;
            _avalaraTaxManager = avalaraTaxManager;
            _avalaraTaxSettings = avalaraTaxSettings;
            _logger = logger;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _addressSettings = addressSettings;
        }

        #endregion

        #region Methods

        public ActionResult ValidateAddress()
        {
            //ensure that Avalara tax provider is active
            //if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
            //    return Content(string.Empty);

            //ensure thet address validation is enabled
            if (!_avalaraTaxSettings.ValidateAddress)
                return Content(string.Empty);

            //validate entered by customer addresses only
            Address address = null;
            if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress)
                address = _workContext.CurrentCustomer.BillingAddress;
            if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress)
                address = _workContext.CurrentCustomer.ShippingAddress;
            if (address == null)
                return Content(string.Empty);

            //validate address
            var validationResult = _avalaraTaxManager.ValidateAddress(new AddressValidationInfo
            {
                city = CommonHelper.EnsureMaximumLength(address.City, 50),
                country = CommonHelper.EnsureMaximumLength(address.Country?.TwoLetterIsoCode, 2),
                line1 = CommonHelper.EnsureMaximumLength(address.Address1, 50),
                line2 = CommonHelper.EnsureMaximumLength(address.Address2, 100),
                postalCode = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 11),
                region = CommonHelper.EnsureMaximumLength(address.StateProvince?.Abbreviation, 3),
                textCase = TextCase.Mixed
            });

            //whether there are errors in validation result
            var errorDetails = validationResult.messages?
                .Where(message => message.severity.Equals("Error", StringComparison.InvariantCultureIgnoreCase) || message.severity.Equals("Exception", StringComparison.InvariantCultureIgnoreCase))
                .Select(message => message.details) ?? new List<string>();
            if (errorDetails.Any())
            {
                var avalaraErrorMessage = errorDetails.FirstOrDefault();

                //log errors
                _logger.Error($"Avalara tax provider error. {avalaraErrorMessage}", customer: _workContext.CurrentCustomer);

                var enteredAddressModel = new AddressModel();
                enteredAddressModel.PrepareModel(
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings);

                var customErrorMessage = ReplaceAvalaraErrorResponseWithCustomOne(avalaraErrorMessage);

                //and display error message to customer
                return PartialView("~/Plugins/Tax.Avalara/Views/Checkout/AddressValidation.cshtml", new AddressValidationModel
                {
                    Message = WebUtility.HtmlEncode(customErrorMessage),
                    IsError = true,
                    Address = enteredAddressModel
                });
            }

            //if there are no errors and no validated addresses, nothing to display
            if (!validationResult.validatedAddresses?.Any() ?? true)
                return Content(string.Empty);

            //get validated address info
            var validatedAddressInfo = validationResult.validatedAddresses.FirstOrDefault();
            var stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(validatedAddressInfo.region);
            //create new address as a copy of address to validate and with details of the validated one
            var validatedAddress = address.Clone() as Address;
            validatedAddress.City = validatedAddressInfo.city;
            validatedAddress.Country = _countryService.GetCountryByTwoLetterIsoCode(validatedAddressInfo.country);
            validatedAddress.Address1 = validatedAddressInfo.line1;
            validatedAddress.Address2 = validatedAddressInfo.line2;
            validatedAddress.ZipPostalCode = validatedAddressInfo.postalCode;
            validatedAddress.StateProvince = stateProvince;
            validatedAddress.StateProvinceId = stateProvince.Id;
            
            //try to find an existing address with the same values
            var existingAddress = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(validatedAddress.FirstName, validatedAddress.LastName, validatedAddress.PhoneNumber,
                validatedAddress.Email, validatedAddress.FaxNumber, validatedAddress.Company,
                validatedAddress.Address1, validatedAddress.Address2, validatedAddress.City,
                validatedAddress.StateProvinceId, validatedAddress.ZipPostalCode,
                validatedAddress.CountryId, validatedAddress.CustomAttributes);

            //if the found address is the same as address to validate, nothing to display
            if (address.Id == existingAddress?.Id)
                return Content(string.Empty);

            //otherwise display to customer a confirmation dialog about address updating
            var model = new AddressValidationModel();
            var shippingAddress = new Address();
            if (existingAddress == null)
            {
                _addressService.InsertAddress(validatedAddress);

                model.IsNewAddress = true;
                shippingAddress = validatedAddress;
            }
            else
            {
                shippingAddress = existingAddress;
            }

            var addressModel = new AddressModel();
            addressModel.PrepareModel(
                address: shippingAddress,
                excludeProperties: false,
                addressSettings: _addressSettings);

            model.Address = addressModel;
            model.AddressId = shippingAddress.Id;
            
            if (!string.Equals(address.Address1, validatedAddress.Address1))
            {
                model.Address1Changed = true;
            }

            if (!string.Equals(address.City, validatedAddress.City))
            {
                model.CityChanged = true;
            }
            
            if (address.StateProvinceId != validatedAddress.StateProvinceId)
            {
                model.StateProvinceNameChanged = true;
            }

            if (!string.Equals(address.ZipPostalCode, validatedAddress.ZipPostalCode))
            {
                model.ZipPostalCodeChanged = true;
            }

            return PartialView("~/Plugins/Tax.Avalara/Views/Checkout/AddressValidation.cshtml", model);
        }

        [HttpPost]
        public ActionResult UseValidatedAddress(int addressId, bool isNewAddress)
        {
            //try to get an address by the passed identifier
            var address = _addressService.GetAddressById(addressId);
            if (address != null)
            {
                //add address to customer collection if it's a new
                if (isNewAddress)
                    _workContext.CurrentCustomer.Addresses.Add(address);

                //and update appropriate customer address
                if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress)
                    _workContext.CurrentCustomer.BillingAddress = address;
                if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress)
                    _workContext.CurrentCustomer.ShippingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            //nothing to return
            return Content(string.Empty);
        }

        private string GetAddressLine(Address address)
        {
            return WebUtility.HtmlEncode($"{(!string.IsNullOrEmpty(address.Address1) ? $"{address.Address1}, " : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(address.Address2) ? $"{address.Address2}, " : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(address.City) ? $"{address.City}, " : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(address.StateProvince?.Name) ? $"{address.StateProvince.Name}, " : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(address.Country?.Name) ? $"{address.Country.Name}, " : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(address.ZipPostalCode) ? $"{address.ZipPostalCode}, " : string.Empty)}"
                .TrimEnd(' ').TrimEnd(','));
        }

        private string ReplaceAvalaraErrorResponseWithCustomOne(string error)
        {
            var result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.GenericErrorMessage");

            if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches"), StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches2"), StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches3"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatchesResponse");
            } 
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases"), StringComparison.InvariantCultureIgnoreCase) ||
                     string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases2"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchasesResponse");
            }            
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetName"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNameResponse");
            }            
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumber"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumberResponse");
            }          
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorCity"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorCityResponse");
            }       
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorZip"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorZipResponse");
            }      
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddress"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddressResponse");
            }     
            else if (string.Equals(error, _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverable"), StringComparison.InvariantCultureIgnoreCase))
            {
                result = _localizationService.GetResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverableResponse");
            }

            return result;
        }

        #endregion
    }
}