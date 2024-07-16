using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AuthorizeNetCheck.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Routing;
using Nop.Core.Domain.Catalog;
using AuthorizeNetSDK = AuthorizeNet;

namespace Nop.Plugin.Payments.AuthorizeNetCheck
{
    /// <summary>
    /// AuthorizeNet payment processor
    /// </summary>
    public class AuthorizeNetCheckPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        private readonly AuthorizeNetCheckPaymentSettings _authorizeNetCheckPaymentSettings;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor

        public AuthorizeNetCheckPaymentProcessor(
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IOrderTotalCalculationService orderTotalCalculationService, 
            ISettingService settingService, 
            IWebHelper webHelper,
            
            AuthorizeNetCheckPaymentSettings authorizeNetCheckPaymentSettings, 
            CurrencySettings currencySettings)
        {
            _currencyService = currencyService;
            _customerService = customerService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _webHelper = webHelper;

            _authorizeNetCheckPaymentSettings = authorizeNetCheckPaymentSettings;
            _currencySettings = currencySettings;
        }

        #endregion
        
        private void PrepareAuthorizeNet()
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = _authorizeNetCheckPaymentSettings.UseSandbox
                ? AuthorizeNetSDK.Environment.SANDBOX
                : AuthorizeNetSDK.Environment.PRODUCTION;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
            {
                name = _authorizeNetCheckPaymentSettings.LoginId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _authorizeNetCheckPaymentSettings.TransactionKey
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static createTransactionResponse GetApiResponse(createTransactionController controller, IList<string> errors)
        {
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.transactionResponse?.errors != null)
                {
                    foreach (var transactionResponseError in response.transactionResponse.errors)
                    {
                        errors.Add($"Error #{transactionResponseError.errorCode}: {transactionResponseError.errorText}");
                    }

                    return null;
                }

                if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    switch (response.transactionResponse.responseCode)
                    {
                        case "1":
                            {
                                return response;
                            }
                        case "2":
                            {
                                var description = response.transactionResponse.messages.Any()
                                    ? response.transactionResponse.messages.First().description
                                    : string.Empty;
                                errors.Add($"Declined ({response.transactionResponse.responseCode}: {description})".TrimEnd(':', ' '));
                                return null;
                            }
                    }
                }
                else if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Error)
                {
                    if (response.messages?.message != null && response.messages.message.Any())
                    {
                        var message = response.messages.message.First();

                        errors.Add($"Error #{message.code}: {message.text}");
                        return null;
                    }
                }
            }
            else
            {
                var error = controller.GetErrorResponse();

                if (error?.messages?.message != null && error.messages.message.Any())
                {
                    var message = error.messages.message.First();

                    errors.Add($"Error #{message.code}: {message.text}");
                    return null;
                }
            }

            var controllerResult = controller.GetResults().FirstOrDefault();

            if (controllerResult?.StartsWith("I00001", StringComparison.InvariantCultureIgnoreCase) ?? false)
                return null;

            const string unknownError = "Authorize.NET unknown error";

            errors.Add(string.IsNullOrEmpty(controllerResult) ? unknownError : $"{unknownError} ({controllerResult})");

            return null;
        }

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            // If the payment method type is mail in or send check via email, we need to just mark the payment as pending as it would be manually processed
            if (processPaymentRequest.CustomValues.ContainsKey("Payment Method Type"))
            {
                var paymentMethodType = processPaymentRequest.CustomValues["Payment Method Type"].ToString();

                if (paymentMethodType == "Mail-In Check" || paymentMethodType == "E-Check via Email")
                {
                    result.NewPaymentStatus = PaymentStatus.Pending;

                    return result;
                }
            }

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            PrepareAuthorizeNet();

            var bankAccount = new bankAccountType
            {
                accountType = bankAccountTypeEnum.checking,
                echeckType = echeckTypeEnum.WEB,
                routingNumber = processPaymentRequest.CustomValues["Bank ABA Routing Number"].ToString(),
                accountNumber = processPaymentRequest.CustomValues["Bank Account Number"].ToString(),
                nameOnAccount = processPaymentRequest.CustomValues["Customer Name"].ToString(),
                bankName = processPaymentRequest.CustomValues["Bank Name"].ToString()
            };

            var paymentType = new paymentType { Item = bankAccount };

            var billTo = new customerAddressType
            {
                firstName = customer.BillingAddress.FirstName,
                lastName = customer.BillingAddress.LastName,
                email = customer.BillingAddress.Email,
                address = customer.BillingAddress.Address1,
                city = customer.BillingAddress.City,
                zip = customer.BillingAddress.ZipPostalCode
            };

            if (!string.IsNullOrEmpty(customer.BillingAddress.Company))
                billTo.company = customer.BillingAddress.Company;

            if (customer.BillingAddress.StateProvince != null)
                billTo.state = customer.BillingAddress.StateProvince.Abbreviation;

            if (customer.BillingAddress.Country != null)
                billTo.country = customer.BillingAddress.Country.TwoLetterIsoCode;


            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                payment = paymentType,
                amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                billTo = billTo,
                customerIP = _webHelper.GetCurrentIpAddress(),
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. here we also pass x_description
                    invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                    description = $"Full order #{processPaymentRequest.OrderGuid}"
                }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller = new createTransactionController(request);
            controller.Execute();

            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            result.CaptureTransactionId = $"{response.transactionResponse.transId}";
            result.AuthorizationTransactionResult = $"Approved ({response.transactionResponse.responseCode}: {response.transactionResponse.messages[0].description})";
            result.AvsResult = response.transactionResponse.avsResultCode;
            result.NewPaymentStatus = PaymentStatus.Authorized;

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, _authorizeNetCheckPaymentSettings.AdditionalFee, _authorizeNetCheckPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            return false;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            result.AddError("Refund method not supported");

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            if (processPaymentRequest.IsRecurringPayment) return result;

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            PrepareAuthorizeNet();

            var bankAccount = new bankAccountType
            {
                accountType = bankAccountTypeEnum.checking,
                echeckType = echeckTypeEnum.WEB,
                routingNumber = processPaymentRequest.CustomValues["routingNumber"].ToString(),
                accountNumber = processPaymentRequest.CustomValues["accountNumber"].ToString(),
                nameOnAccount = processPaymentRequest.CustomValues["nameOnAccount"].ToString(),
                bankName = processPaymentRequest.CustomValues["bankName"].ToString()
            };

            var paymentType = new paymentType { Item = bankAccount };

            var billTo = new nameAndAddressType
            {
                firstName = customer.BillingAddress.FirstName,
                lastName = customer.BillingAddress.LastName,
                //email = customer.BillingAddress.Email,
                address = customer.BillingAddress.Address1,
                //address = customer.BillingAddress.Address1 + " " + customer.BillingAddress.Address2;
                city = customer.BillingAddress.City,
                zip = customer.BillingAddress.ZipPostalCode
            };

            if (!string.IsNullOrEmpty(customer.BillingAddress.Company))
                billTo.company = customer.BillingAddress.Company;

            if (customer.BillingAddress.StateProvince != null)
                billTo.state = customer.BillingAddress.StateProvince.Abbreviation;

            if (customer.BillingAddress.Country != null)
                billTo.country = customer.BillingAddress.Country.TwoLetterIsoCode;

            var dtNow = DateTime.UtcNow;

            // Interval can't be updated once a subscription is created.
            var paymentScheduleInterval = new paymentScheduleTypeInterval();
            switch (processPaymentRequest.RecurringCyclePeriod)
            {
                case RecurringProductCyclePeriod.Days:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Weeks:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 7);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Months:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                case RecurringProductCyclePeriod.Years:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 12);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                default:
                    throw new NopException("Not supported cycle period");
            }

            var paymentSchedule = new paymentScheduleType
            {
                startDate = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day),
                totalOccurrences = Convert.ToInt16(processPaymentRequest.RecurringTotalCycles),
                interval = paymentScheduleInterval
            };

            var subscriptionType = new ARBSubscriptionType
            {
                name = processPaymentRequest.OrderGuid.ToString(),
                amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                payment = paymentType,
                billTo = billTo,
                paymentSchedule = paymentSchedule,
                customer = new customerType
                {
                    email = customer.BillingAddress.Email
                },

                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. hece we also pass x_description
                    invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                    description = $"Recurring payment #{processPaymentRequest.OrderGuid}"
                }
            };

            if (customer.ShippingAddress != null)
            {
                var shipTo = new nameAndAddressType
                {
                    firstName = customer.ShippingAddress.FirstName,
                    lastName = customer.ShippingAddress.LastName,
                    address = customer.ShippingAddress.Address1,
                    city = customer.ShippingAddress.City,
                    zip = customer.ShippingAddress.ZipPostalCode
                };

                if (customer.ShippingAddress.StateProvince != null)
                {
                    shipTo.state = customer.ShippingAddress.StateProvince.Abbreviation;
                }

                subscriptionType.shipTo = shipTo;
            }

            var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

            // instantiate the contoller that will call the servicecu
            var controller = new ARBCreateSubscriptionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                result.SubscriptionTransactionId = response.subscriptionId;
                result.AuthorizationTransactionCode = response.refId;
                result.AuthorizationTransactionResult = $"Approved ({response.refId}: {response.subscriptionId})";
            }
            else if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError($"Error processing recurring payment #{responseMessage.code}: {responseMessage.text}");
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            PrepareAuthorizeNet();

            var request = new ARBCancelSubscriptionRequest { subscriptionId = cancelPaymentRequest.Order.SubscriptionTransactionId };
            var controller = new ARBCancelSubscriptionController(request);
            controller.Execute();

            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                return result;

            if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError($"Error processing recurring payment #{responseMessage.code}: {responseMessage.text}");
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentAuthorizeNetCheck";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AuthorizeNetCheck.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentAuthorizeNetCheck";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AuthorizeNetCheck.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAuthorizeNetCheckController);
        }

        public override void Install()
        {
            //settings
            var settings = new AuthorizeNetCheckPaymentSettings
            {
                UseSandbox = true,
                TransactionKey = "123",
                LoginId = "456"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Notes", "If you're using this gateway, ensure that your primary store currency is supported by Authorize.NET.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.TransactionKey", "Transaction key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.TransactionKey.Hint", "Specify transaction key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.LoginId", "Login ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.LoginId.Hint", "Specify login identifier.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNetCheck.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");

            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.RoutingNumber", "Bank ABA Routing Number");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.RoutingNumber.Invalid", "Bank ABA Routing Number is required and must be up to 9 digits");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.AccountNumber", "Bank Account Number");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.AccountNumber.Invalid", "Bank Account Number is required and must be up to 17 digits");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.NameOnAccount", "Customer Name");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.NameOnAccount.Invalid", "Customer name is required and must be up to 22 characters");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.BankName", "Bank Name");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.BankName.Invalid", "Bank name must be up to 50 characters");
            this.AddOrUpdatePluginLocaleResource("Payment.AuthorizeNetCheck.CheckViaEmail", "I would like to send my E-Check via email.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AuthorizeNetCheckPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Notes");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.TransactionKey");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.TransactionKey.Hint");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.LoginId");
                this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.LoginId.Hint");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.PaymentsAuthorizeNetCheck.Fields.AdditionalFeePercentage.Hint");
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        #endregion
    }
}
