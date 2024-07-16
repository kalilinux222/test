using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.Avalara.Domain;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace Nop.Plugin.Tax.Avalara.Services
{
    /// <summary>
    /// Represents overridden order processing service
    /// </summary>
    public class OverriddenOrderProcessingService : OrderProcessingService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly CurrencySettings _currencySettings;
        private readonly IAffiliateService _affiliateService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly HttpContextBase _httpContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public OverriddenOrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
            IPdfService pdfService,
            IRewardPointService rewardPointService,
            IGenericAttributeService genericAttributeService,
            ShippingSettings shippingSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService, HttpContextBase httpContext)
            : base(orderService, webHelper, localizationService, languageService, productService, paymentService, logger, orderTotalCalculationService, priceCalculationService, priceFormatter, productAttributeParser, productAttributeFormatter, giftCardService, shoppingCartService, checkoutAttributeFormatter, shippingService, shipmentService, taxService, customerService, discountService, encryptionService, workContext, workflowMessageService, vendorService, customerActivityService, currencyService, affiliateService, eventPublisher, pdfService, rewardPointService, genericAttributeService, shippingSettings, paymentSettings, rewardPointsSettings, orderSettings, taxSettings, localizationSettings, currencySettings)
        {
            _orderService = orderService;
            _currencySettings = currencySettings;
            _affiliateService = affiliateService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _httpContext = httpContext;
            _taxService = taxService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected override PlaceOrderContainter PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();

            //Recurring orders. Load initial order
            if (processPaymentRequest.IsRecurringPayment)
            {
                details.InitialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                if (details.InitialOrder == null)
                    throw new ArgumentException("Initial order is not set for recurring payment");

        processPaymentRequest.PaymentMethodSystemName = details.InitialOrder.PaymentMethodSystemName;
            }

    //customer
    details.Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

    //affiliate
    var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //customer currency
            if (!processPaymentRequest.IsRecurringPayment)
            {
                var currencyTmp = _currencyService.GetCurrencyById(details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
    var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
    details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
                var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
    details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
            }
            else
            {
                details.CustomerCurrencyCode = details.InitialOrder.CustomerCurrencyCode;
                details.CustomerCurrencyRate = details.InitialOrder.CurrencyRate;
            }

            //customer language
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.Customer.GetAttribute<int>(
                    SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
            }
            else
            {
                details.CustomerLanguage = _languageService.GetLanguageById(details.InitialOrder.CustomerLanguageId);
            }
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = _workContext.WorkingLanguage;

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new NopException("Anonymous checkout is not allowed");

            //billing address
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (details.Customer.BillingAddress == null)
                    throw new NopException("Billing address is not provided");

                if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
                    throw new NopException("Email is not valid");

//clone billing address
details.BillingAddress = (Address) details.Customer.BillingAddress.Clone();
                if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
                    throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }
            else
            {
                if (details.InitialOrder.BillingAddress == null)
                    throw new NopException("Billing address is not available");

//clone billing address
details.BillingAddress = (Address) details.InitialOrder.BillingAddress.Clone();
                if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
                    throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }

            //checkout attributes
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.CheckoutAttributesXml = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
                details.CheckoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);
            }
            else
            {
                details.CheckoutAttributesXml = details.InitialOrder.CheckoutAttributesXml;
                details.CheckoutAttributeDescription = details.InitialOrder.CheckoutAttributeDescription;
            }

            //load and validate customer shopping cart
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //load shopping cart
                details.Cart = details.Customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(processPaymentRequest.StoreId)
                    .ToList();

                if (details.Cart.Count == 0)
                    throw new NopException("Cart is empty");

//validate the entire shopping cart
var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart,
    details.CheckoutAttributesXml,
    true);
                if (warnings.Count > 0)
                {
                    var warningsSb = new StringBuilder();
                    foreach (string warning in warnings)
                    {
                        warningsSb.Append(warning);
                        warningsSb.Append(";");
                    }
                    throw new NopException(warningsSb.ToString());
                }

                //validate individual cart items
                foreach (var sci in details.Cart)
                {
                    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci.ShoppingCartType,
                        sci.Product, processPaymentRequest.StoreId, sci.AttributesXml,
                        sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                        sci.Quantity, false);
                    if (sciWarnings.Count > 0)
                    {
                        var warningsSb = new StringBuilder();
                        foreach (string warning in sciWarnings)
                        {
                            warningsSb.Append(warning);
                            warningsSb.Append(";");
                        }
                        throw new NopException(warningsSb.ToString());
                    }
                }
            }

            //min totals validation
            if (!processPaymentRequest.IsRecurringPayment)
            {
                bool minOrderSubtotalAmountOk = ValidateMinOrderSubtotalAmount(details.Cart);
                if (!minOrderSubtotalAmountOk)
                {
                    decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
                }
                bool minOrderTotalAmountOk = ValidateMinOrderTotalAmount(details.Cart);
                if (!minOrderTotalAmountOk)
                {
                    decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                    throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
                }
            }

            //tax display type
            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    details.CustomerTaxDisplayType = (TaxDisplayType) details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
                else
                    details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;
            }
            else
            {
                details.CustomerTaxDisplayType = details.InitialOrder.CustomerTaxDisplayType;
            }
            
            //sub total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //sub total (incl tax)
                decimal orderSubTotalDiscountAmount1;
Discount orderSubTotalAppliedDiscount1;
decimal subTotalWithoutDiscountBase1;
decimal subTotalWithDiscountBase1;
_orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart,
                    true, out orderSubTotalDiscountAmount1, out orderSubTotalAppliedDiscount1,
                    out subTotalWithoutDiscountBase1, out subTotalWithDiscountBase1);
                details.OrderSubTotalInclTax = subTotalWithoutDiscountBase1;
                details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount1;

                //discount history
                if (orderSubTotalAppliedDiscount1 != null && !details.AppliedDiscounts.ContainsDiscount(orderSubTotalAppliedDiscount1))
                    details.AppliedDiscounts.Add(orderSubTotalAppliedDiscount1);

                //sub total (excl tax)
                decimal orderSubTotalDiscountAmount2;
Discount orderSubTotalAppliedDiscount2;
decimal subTotalWithoutDiscountBase2;
decimal subTotalWithDiscountBase2;
_orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart,
                    false, out orderSubTotalDiscountAmount2, out orderSubTotalAppliedDiscount2,
                    out subTotalWithoutDiscountBase2, out subTotalWithDiscountBase2);
                details.OrderSubTotalExclTax = subTotalWithoutDiscountBase2;
                details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount2;
            }
            else
            {
                details.OrderSubTotalInclTax = details.InitialOrder.OrderSubtotalInclTax;
                details.OrderSubTotalExclTax = details.InitialOrder.OrderSubtotalExclTax;
            }


            //shipping info
            bool shoppingCartRequiresShipping;
            if (!processPaymentRequest.IsRecurringPayment)
            {
                shoppingCartRequiresShipping = details.Cart.RequiresShipping();
            }
            else
            {
                shoppingCartRequiresShipping = details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
            }
            if (shoppingCartRequiresShipping)
            {
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    details.PickUpInStore = _shippingSettings.AllowPickUpInStore &&
                        details.Customer.GetAttribute<bool>(SystemCustomerAttributeNames.SelectedPickUpInStore, processPaymentRequest.StoreId);

                    if (!details.PickUpInStore)
                    {
                        if (details.Customer.ShippingAddress == null)
                            throw new NopException("Shipping address is not provided");

                        if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
                            throw new NopException("Email is not valid");

//clone shipping address
details.ShippingAddress = (Address) details.Customer.ShippingAddress.Clone();
                        if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                        {
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                        }
                    }

                    var shippingOption = details.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                    if (shippingOption != null)
                    {
                        details.ShippingMethodName = shippingOption.Name;
                        details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                    }
                }
                else
                {
                    details.PickUpInStore = details.InitialOrder.PickUpInStore;
                    if (!details.PickUpInStore)
                    {
                        if (details.InitialOrder.ShippingAddress == null)
                            throw new NopException("Shipping address is not available");

//clone shipping address
details.ShippingAddress = (Address) details.InitialOrder.ShippingAddress.Clone();
                        if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                        {
                            throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                        }
                    }

                    details.ShippingMethodName = details.InitialOrder.ShippingMethod;
                    details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
                }
            }
            details.ShippingStatus = shoppingCartRequiresShipping
                ? ShippingStatus.NotYetShipped
                : ShippingStatus.ShippingNotRequired;

            //shipping total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                decimal taxRate;
Discount shippingTotalDiscount;
decimal? orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out taxRate, out shippingTotalDiscount);
decimal? orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);
                if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                    throw new NopException("Shipping total couldn't be calculated");
details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
                details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

                if (shippingTotalDiscount != null && !details.AppliedDiscounts.ContainsDiscount(shippingTotalDiscount))
                    details.AppliedDiscounts.Add(shippingTotalDiscount);
            }
            else
            {
                details.OrderShippingTotalInclTax = details.InitialOrder.OrderShippingInclTax;
                details.OrderShippingTotalExclTax = details.InitialOrder.OrderShippingExclTax;
            }


            //payment total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                decimal paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
details.PaymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer);
                details.PaymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer);
            }
            else
            {
                details.PaymentAdditionalFeeInclTax = details.InitialOrder.PaymentMethodAdditionalFeeInclTax;
                details.PaymentAdditionalFeeExclTax = details.InitialOrder.PaymentMethodAdditionalFeeExclTax;
            }


            //tax total
            if (!processPaymentRequest.IsRecurringPayment)
            {
                //tax amount
                SortedDictionary<decimal, decimal> taxRatesDictionary;
details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out taxRatesDictionary);

                //VAT number
                var customerVatStatus = (VatNumberStatus)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
                if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                    details.VatNumber = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                //tax rates
                foreach (var kvp in taxRatesDictionary)
                {
                    var taxRate = kvp.Key;
var taxValue = kvp.Value;
details.TaxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                details.OrderTaxTotal = details.InitialOrder.OrderTax;
                //VAT number
                details.VatNumber = details.InitialOrder.VatNumber;
            }


            //order total (and applied discounts, gift cards, reward points)
            if (!processPaymentRequest.IsRecurringPayment)
            {
                List<AppliedGiftCard> appliedGiftCards;
Discount orderAppliedDiscount;
decimal orderDiscountAmount;
int redeemedRewardPoints;
decimal redeemedRewardPointsAmount;

var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart,
    out orderDiscountAmount, out orderAppliedDiscount, out appliedGiftCards,
    out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (!orderTotal.HasValue)
                    throw new NopException("Order total couldn't be calculated");

details.OrderDiscountAmount = orderDiscountAmount;
                details.RedeemedRewardPoints = redeemedRewardPoints;
                details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
                details.AppliedGiftCards = appliedGiftCards;
                details.OrderTotal = orderTotal.Value;

                //discount history
                if (orderAppliedDiscount != null && !details.AppliedDiscounts.ContainsDiscount(orderAppliedDiscount))
                    details.AppliedDiscounts.Add(orderAppliedDiscount);
            }
            else
            {
                details.OrderDiscountAmount = details.InitialOrder.OrderDiscount;
                details.OrderTotal = details.InitialOrder.OrderTotal;
            } 
            processPaymentRequest.OrderTotal = details.OrderTotal;

            //recurring or standard shopping cart?
            if (!processPaymentRequest.IsRecurringPayment)
            {
                details.IsRecurringShoppingCart = details.Cart.IsRecurring();
                if (details.IsRecurringShoppingCart)
                {
                    int recurringCycleLength;
RecurringProductCyclePeriod recurringCyclePeriod;
int recurringTotalCycles;
string recurringCyclesError = details.Cart.GetRecurringCycleInfo(_localizationService,
    out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
                    if (!string.IsNullOrEmpty(recurringCyclesError))
                        throw new NopException(recurringCyclesError);

processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                    processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                    processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
                }
            }
            else
            {
                details.IsRecurringShoppingCart = true;
            }

            //Avalara plugin changes
            //delete custom value
            _httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] = null;
            //Avalara plugin changes

            return details;
        }
        //protected override PlaceOrderContainter PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        //{
        //    var details = new PlaceOrderContainter
        //    {
        //        //customer
        //        Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId)
        //    };
        //    if (details.Customer == null)
        //        throw new ArgumentException("Customer is not set");

        //    //affiliate
        //    var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
        //    if (affiliate != null && affiliate.Active && !affiliate.Deleted)
        //        details.AffiliateId = affiliate.Id;

        //    //check whether customer is guest
        //    if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
        //        throw new NopException("Anonymous checkout is not allowed");

        //    //customer currency
        //    var currencyTmp = _currencyService.GetCurrencyById(
        //        details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
        //    var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : _workContext.WorkingCurrency;
        //    var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
        //    details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
        //    details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

        //    //customer language
        //    details.CustomerLanguage = _languageService.GetLanguageById(
        //        details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
        //    if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
        //        details.CustomerLanguage = _workContext.WorkingLanguage;

        //    //billing address
        //    if (details.Customer.BillingAddress == null)
        //        throw new NopException("Billing address is not provided");

        //    if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
        //        throw new NopException("Email is not valid");

        //    details.BillingAddress = (Address)details.Customer.BillingAddress.Clone();
        //    if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
        //        throw new NopException($"Country '{details.BillingAddress.Country.Name}' is not allowed for billing");

        //    //checkout attributes
        //    details.CheckoutAttributesXml = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
        //    details.CheckoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);

        //    //load shopping cart
        //    details.Cart = details.Customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
        //        .LimitPerStore(processPaymentRequest.StoreId).ToList();

        //    if (!details.Cart.Any())
        //        throw new NopException("Cart is empty");

        //    //validate the entire shopping cart
        //    var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart, details.CheckoutAttributesXml, true);
        //    if (warnings.Any())
        //        throw new NopException(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));

        //    //validate individual cart items
        //    foreach (var sci in details.Cart)
        //    {
        //        var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci.ShoppingCartType,
        //            sci.Product, processPaymentRequest.StoreId, sci.AttributesXml,
        //            sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc,
        //            sci.Quantity, false);
        //        if (sciWarnings.Any())
        //            throw new NopException(sciWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
        //    }

        //    //min totals validation
        //    if (!ValidateMinOrderSubtotalAmount(details.Cart))
        //    {
        //        var minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
        //        throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"),
        //            _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
        //    }

        //    if (!ValidateMinOrderTotalAmount(details.Cart))
        //    {
        //        var minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
        //        throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"),
        //            _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
        //    }

        //    //tax display type
        //    if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
        //        details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
        //    else
        //        details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;

        //    //sub total (incl tax)
        //    _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, true, out var orderSubTotalDiscountAmount, out var orderSubTotalAppliedDiscount, out var subTotalWithoutDiscountBase, out var _);
        //    details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
        //    details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

        //    //discount history
        //    if (!details.AppliedDiscounts.ContainsDiscount(orderSubTotalAppliedDiscount))
        //        details.AppliedDiscounts.Add(orderSubTotalAppliedDiscount);

        //    //sub total (excl tax)
        //    _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false, out orderSubTotalDiscountAmount,
        //        out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out _);
        //    details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
        //    details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;


        //    //shipping info
        //    bool shoppingCartRequiresShipping;
        //    if (!processPaymentRequest.IsRecurringPayment)
        //    {
        //        shoppingCartRequiresShipping = details.Cart.RequiresShipping();
        //    }
        //    else
        //    {
        //        shoppingCartRequiresShipping = details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
        //    }
        //    if (shoppingCartRequiresShipping)
        //    {
        //        if (!processPaymentRequest.IsRecurringPayment)
        //        {
        //            details.PickUpInStore = _shippingSettings.AllowPickUpInStore &&
        //                details.Customer.GetAttribute<bool>(SystemCustomerAttributeNames.SelectedPickUpInStore, processPaymentRequest.StoreId);

        //            if (!details.PickUpInStore)
        //            {
        //                if (details.Customer.ShippingAddress == null)
        //                    throw new NopException("Shipping address is not provided");

        //                if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
        //                    throw new NopException("Email is not valid");

        //                //clone shipping address
        //                details.ShippingAddress = (Address)details.Customer.ShippingAddress.Clone();
        //                if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
        //                {
        //                    throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
        //                }
        //            }

        //            var shippingOption = details.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
        //            if (shippingOption != null)
        //            {
        //                details.ShippingMethodName = shippingOption.Name;
        //                details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
        //            }
        //        }
        //        else
        //        {
        //            details.PickUpInStore = details.InitialOrder.PickUpInStore;
        //            if (!details.PickUpInStore)
        //            {
        //                if (details.InitialOrder.ShippingAddress == null)
        //                    throw new NopException("Shipping address is not available");

        //                //clone shipping address
        //                details.ShippingAddress = (Address)details.InitialOrder.ShippingAddress.Clone();
        //                if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
        //                {
        //                    throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
        //                }
        //            }

        //            details.ShippingMethodName = details.InitialOrder.ShippingMethod;
        //            details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
        //        }
        //    }
        //    details.ShippingStatus = shoppingCartRequiresShipping
        //        ? ShippingStatus.NotYetShipped
        //        : ShippingStatus.ShippingNotRequired;

        //    //shipping total
        //    if (!processPaymentRequest.IsRecurringPayment)
        //    {
        //        decimal taxRate;
        //        Discount shippingTotalDiscount;
        //        decimal? orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out taxRate, out shippingTotalDiscount);
        //        decimal? orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);
        //        if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
        //            throw new NopException("Shipping total couldn't be calculated");
        //        details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
        //        details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

        //        if (shippingTotalDiscount != null && !details.AppliedDiscounts.ContainsDiscount(shippingTotalDiscount))
        //            details.AppliedDiscounts.Add(shippingTotalDiscount);
        //    }
        //    else
        //    {
        //        details.OrderShippingTotalInclTax = details.InitialOrder.OrderShippingInclTax;
        //        details.OrderShippingTotalExclTax = details.InitialOrder.OrderShippingExclTax;
        //    }

        //    //payment total
        //    var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
        //    details.PaymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer);
        //    details.PaymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer);

        //    //tax amount
        //    details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out var taxRatesDictionary);

        //    //Avalara plugin changes
        //    //get previously saved tax details received from the Avalara tax service
        //    var taxDetails = _httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] as TaxDetails;

        //    if (taxDetails != null)
        //    {
        //        //adjust tax total according to received value from the Avalara
        //        if (taxDetails.TaxTotal.HasValue)
        //            details.OrderTaxTotal = taxDetails.TaxTotal.Value;

        //        if (taxDetails.TaxRates?.Any() ?? false)
        //            taxRatesDictionary = new SortedDictionary<decimal, decimal>(taxDetails.TaxRates);
        //    }
        //    //Avalara plugin changes

        //    //VAT number
        //    var customerVatStatus = (VatNumberStatus)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
        //    if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
        //        details.VatNumber = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

        //    //tax rates
        //    details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
        //        $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");

        //    //order total (and applied discounts, gift cards, reward points)
        //    var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart, out var orderDiscountAmount, out var orderAppliedDiscount, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
        //    if (!orderTotal.HasValue)
        //        throw new NopException("Order total couldn't be calculated");

        //    details.OrderDiscountAmount = orderDiscountAmount;
        //    details.RedeemedRewardPoints = redeemedRewardPoints;
        //    details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
        //    details.AppliedGiftCards = appliedGiftCards;
        //    details.OrderTotal = orderTotal.Value;

        //    //discount history
        //    if (!details.AppliedDiscounts.ContainsDiscount(orderAppliedDiscount))
        //        details.AppliedDiscounts.Add(orderAppliedDiscount);

        //    processPaymentRequest.OrderTotal = details.OrderTotal;

        //    //recurring or standard shopping cart?
        //    if (!processPaymentRequest.IsRecurringPayment)
        //    {
        //        details.IsRecurringShoppingCart = details.Cart.IsRecurring();
        //        if (details.IsRecurringShoppingCart)
        //        {
        //            int recurringCycleLength;
        //            RecurringProductCyclePeriod recurringCyclePeriod;
        //            int recurringTotalCycles;
        //            string recurringCyclesError = details.Cart.GetRecurringCycleInfo(_localizationService,
        //                out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
        //            if (!string.IsNullOrEmpty(recurringCyclesError))
        //                throw new NopException(recurringCyclesError);

        //            processPaymentRequest.RecurringCycleLength = recurringCycleLength;
        //            processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
        //            processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
        //        }
        //    }
        //    else
        //    {
        //        details.IsRecurringShoppingCart = true;
        //    }

        //    //Avalara plugin changes
        //    //delete custom value
        //    _httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] = null;
        //    //Avalara plugin changes

        //    return details;
        //}

        #endregion
    }
}