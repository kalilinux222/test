using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers
{
    public class CustomerHelper : ICustomerHelper
    {
        private const int UNITED_STATES_COUNTRY_ID = 1;
        private const int REGISTERED_USER_CUSTOMER_ROLE_ID = 3;

        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;

        public CustomerHelper(ICustomerService customerService,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService)
        {
            _customerService = customerService;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
        }

        public CustomerCreationResult CreateFurnitureLeisureCustomer(CatalogRequestModel model)
        {
            var result = new CustomerCreationResult();

            var customer = _customerService.GetCustomerByEmail(model.Email);

            if(customer != null)
            {
                result.Success = false;

                result.Errors.Add(_localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.RegisterCustomer.Error.CustomerExists"));

                return result;
            }

            var newCustomer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = model.Email,
                Username = string.Empty,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow
            };

            _customerService.InsertCustomer(newCustomer);

            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.TimeZoneId, _dateTimeSettings.DefaultStoreTimeZoneId);
            }

            _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.LastName, model.LastName);

            if (_customerSettings.CompanyEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.Company, model.CompanyName);
            }

            if (_customerSettings.StreetAddressEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            }

            if (_customerSettings.CityEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.City, model.City);
            }

            if (_customerSettings.CountryEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.CountryId, UNITED_STATES_COUNTRY_ID);
            }

            if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.StateProvinceId, model.StateId);
            }

            if (_customerSettings.PhoneEnabled)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.Phone, model.PhoneNumber);
            }

            if (!String.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                if (!changePassResult.Success)
                {
                    result.Success = false;

                    foreach (var changePassError in changePassResult.Errors)
                    {
                        result.Errors.Add(changePassError);
                    }

                    return result;
                }
            }

            var registeredCustomerRole = _customerService.GetAllCustomerRoles(true).FirstOrDefault(x => x.Id == REGISTERED_USER_CUSTOMER_ROLE_ID);

            newCustomer.CustomerRoles.Add(registeredCustomerRole);

            _customerService.UpdateCustomer(newCustomer);

            _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), newCustomer.Id);

            switch (_customerSettings.UserRegistrationType)
            {
                case UserRegistrationType.EmailValidation:
                {
                    //email validation message
                    _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                    _workflowMessageService.SendCustomerEmailValidationMessage(newCustomer, _workContext.WorkingLanguage.Id);

                    break;
                }
                case UserRegistrationType.AdminApproval:
                {
                    break;
                }
                case UserRegistrationType.Standard:
                {
                    //send customer welcome message
                    _workflowMessageService.SendCustomerWelcomeMessage(newCustomer, _workContext.WorkingLanguage.Id);

                    break;
                }
            }

            result.Success = true;

            return result;
        }
    }
}
