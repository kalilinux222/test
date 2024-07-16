﻿using Avalara.AvaTax.RestClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Plugin.Tax.Avalara.Data;
using Nop.Plugin.Tax.Avalara.Domain;
using Nop.Plugin.Tax.Avalara.Services;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Tax.Avalara
{
    /// <summary>
    /// Represents Avalara tax provider
    /// </summary>
    public class AvalaraTaxProvider : BasePlugin, ITaxProvider
    {
        #region Fields

        private readonly AvalaraTaxManager _avalaraTaxManager;
        private readonly AvalaraTaxSettings _avalaraTaxSettings;
        private readonly IAddressService _addressService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly TaxTransactionLogObjectContext _objectContext;
        private readonly WidgetSettings _widgetSettings;
        private ICacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public AvalaraTaxProvider(AvalaraTaxManager avalaraTaxManager,
            AvalaraTaxSettings avalaraTaxSettings,
            IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService,
            ILocalizationService localizationService,
            IProductService productService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ITaxCategoryService taxCategoryService,
            ITaxService taxService,
            IWebHelper webHelper,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            TaxTransactionLogObjectContext objectContext,
            WidgetSettings widgetSettings)
        {
            this._avalaraTaxManager = avalaraTaxManager;
            this._avalaraTaxSettings = avalaraTaxSettings;
            this._addressService = addressService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeService = checkoutAttributeService;
            this._countryService = countryService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._geoLookupService = geoLookupService;
            this._localizationService = localizationService;
            this._productService = productService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._taxCategoryService = taxCategoryService;
            this._taxService = taxService;
            this._webHelper = webHelper;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._objectContext = objectContext;
            this._widgetSettings = widgetSettings;
        }

        private ICacheManager StaticCacheManager
        {
            get
            {
                if (_staticCacheManager == null)
                {
                    _staticCacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
                }

                return _staticCacheManager;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare request parameters to get an estimated (not finally calculated) tax 
        /// </summary>
        /// <param name="destinationAddress">Destination tax address</param>
        /// <param name="customerCode">Customer code</param>
        /// <returns>Request parameters to create tax transaction</returns>
        private CreateTransactionModel PrepareEstimatedTaxModel(Address destinationAddress, string customerCode)
        {
            //prepare common parameters
            var transactionModel = PrepareTaxModel(destinationAddress, customerCode, false);

            //create a simplified item line
            transactionModel.lines.Add(new LineItemModel
            {
                amount = 100,
                quantity = 1
            });

            return transactionModel;
        }

        /// <summary>
        /// Prepare request parameters to get a tax for the order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="save">Whether to save tax transaction</param>
        /// <returns>Request parameters to create tax transaction</returns>
        private CreateTransactionModel PrepareOrderTaxModel(Order order, bool save)
        {
            //prepare common parameters
            var transactionModel = PrepareTaxModel(GetOrderDestinationAddress(order), order.Customer?.Id.ToString(), save);

            //prepare specific order parameters
            transactionModel.code = CommonHelper.EnsureMaximumLength(save ? order.Id.ToString() : Guid.NewGuid().ToString(), 50);
            transactionModel.commit = save && _avalaraTaxSettings.CommitTransactions;
            transactionModel.discount = order.OrderSubTotalDiscountExclTax;
            transactionModel.email = CommonHelper.EnsureMaximumLength(order.BillingAddress?.Email, 50);

            //set purchased item lines
            transactionModel.lines = GetItemLines(order);

            //set whole request tax exemption
            var exemptedCustomerRole = order.Customer?.CustomerRoles.FirstOrDefault(role => role.Active && role.TaxExempt);
            if (order.Customer?.IsTaxExempt ?? false)
                transactionModel.exemptionNo = CommonHelper.EnsureMaximumLength($"Exempt-customer-#{order.Customer?.Id}", 25);
            else if (!string.IsNullOrEmpty(exemptedCustomerRole?.Name))
                transactionModel.exemptionNo = CommonHelper.EnsureMaximumLength($"Exempt-{exemptedCustomerRole.Name}", 25);

            //whether entity use code is set
            var entityUseCode = order.Customer != null
                ? order.Customer.GetAttribute<string>(AvalaraTaxDefaults.EntityUseCodeAttribute)
                : exemptedCustomerRole != null
                ? exemptedCustomerRole.GetAttribute<string>(AvalaraTaxDefaults.EntityUseCodeAttribute)
                : null;
            if (!string.IsNullOrEmpty(entityUseCode))
                transactionModel.customerUsageType = CommonHelper.EnsureMaximumLength(entityUseCode, 25);

            return transactionModel;
        }

        /// <summary>
        /// Prepare common request parameters to get a tax
        /// </summary>
        /// <param name="destinationAddress">Destination tax address</param>
        /// <param name="customerCode">Customer code</param>
        /// <param name="save">Whether to save tax transaction</param>
        /// <returns>Request parameters to create tax transaction</returns>
        private CreateTransactionModel PrepareTaxModel(Address destinationAddress, string customerCode, bool save)
        {
            //prepare request parameters
            var transactionModel = new CreateTransactionModel
            {
                addresses = new AddressesModel(),
                customerCode = CommonHelper.EnsureMaximumLength(customerCode, 50),
                date = DateTime.UtcNow,
                lines = new List<LineItemModel>(),
                type = save ? DocumentType.SalesInvoice : DocumentType.SalesOrder
            };

            //set company code
            var companyCode = !string.IsNullOrEmpty(_avalaraTaxSettings.CompanyCode)
                && !_avalaraTaxSettings.CompanyCode.Equals(Guid.Empty.ToString())
                ? _avalaraTaxSettings.CompanyCode : null;
            transactionModel.companyCode = CommonHelper.EnsureMaximumLength(companyCode, 25);

            //set destination and origin addresses
            transactionModel.addresses = GetModelAddresses(destinationAddress);

            return transactionModel;
        }

        /// <summary>
        /// Get tax origin and tax destination addresses
        /// </summary>
        /// <param name="destinationAddress">Destination address</param>
        /// <returns>Addresses</returns>
        private AddressesModel GetModelAddresses(Address destinationAddress)
        {
            var addresses = new AddressesModel();

            //get tax origin address
            var originAddress = _avalaraTaxSettings.TaxOriginAddressType == TaxOriginAddressType.ShippingOrigin
                ? _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId)
                : _avalaraTaxSettings.TaxOriginAddressType == TaxOriginAddressType.DefaultTaxAddress
                ? _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId)
                : null;

            //set destination and origin addresses
            var shipFromAddress = MapAddress(originAddress);
            var shipToAddress = MapAddress(destinationAddress);
            if (shipFromAddress != null && shipToAddress != null)
            {
                addresses.shipFrom = shipFromAddress;
                addresses.shipTo = shipToAddress;
            }
            else
                addresses.singleLocation = shipToAddress ?? shipFromAddress;

            return addresses;
        }

        /// <summary>
        /// Map nopCommerce address to Avalara model address info
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>Address info</returns>
        private AddressLocationInfo MapAddress(Address address)
        {
            return address == null ? null : new AddressLocationInfo
            {
                city = CommonHelper.EnsureMaximumLength(address.City, 50),
                country = CommonHelper.EnsureMaximumLength(address.Country?.TwoLetterIsoCode, 2),
                line1 = CommonHelper.EnsureMaximumLength(address.Address1, 50),
                line2 = CommonHelper.EnsureMaximumLength(address.Address2, 100),
                postalCode = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 11),
                region = CommonHelper.EnsureMaximumLength(address.StateProvince?.Abbreviation, 3)
            };
        }

        /// <summary>
        /// Get a tax destination address of the passed order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Address</returns>
        private Address GetOrderDestinationAddress(Order order)
        {
            Address destinationAddress = null;

            //tax is based on billing address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress)
                destinationAddress = order.BillingAddress;

            //tax is based on shipping address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress)
                destinationAddress = order.ShippingAddress;

            //or use default address for tax calculation
            if (destinationAddress == null)
                destinationAddress = _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId);

            return destinationAddress;
        }

        /// <summary>
        /// Get item lines to create tax transaction
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>List of item lines</returns>
        private List<LineItemModel> GetItemLines(Order order)
        {
            //get purchased products details
            var items = CreateLinesForOrderItems(order).ToList();

            //set payment method additional fee as the separate item line
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                items.Add(CreateLineForPaymentMethod(order));

            //set shipping rate as the separate item line
            if (order.OrderShippingExclTax > decimal.Zero)
                items.Add(CreateLineForShipping(order));

            //set checkout attributes as the separate item lines
            if (!string.IsNullOrEmpty(order.CheckoutAttributesXml))
                items.AddRange(CreateLinesForCheckoutAttributes(order));

            return items;
        }

        /// <summary>
        /// Create item lines for purchased order items
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Collection of item lines</returns>
        private IEnumerable<LineItemModel> CreateLinesForOrderItems(Order order)
        {
            return order.OrderItems.Select(orderItem =>
            {
                var item = new LineItemModel
                {
                    amount = orderItem.PriceExclTax,

                    //item description
                    description = CommonHelper.EnsureMaximumLength(orderItem.Product?.ShortDescription ?? orderItem.Product?.Name, 2096),

                    //whether the discount to the item was applied
                    discounted = order.OrderSubTotalDiscountExclTax > decimal.Zero,

                    //product exemption
                    exemptionCode = orderItem.Product?.IsTaxExempt ?? false
                        ? CommonHelper.EnsureMaximumLength($"Exempt-product-#{orderItem.Product.Id}", 25) : null,

                    //set SKU as item code
                    itemCode = CommonHelper.EnsureMaximumLength(orderItem.Product != null ?
                        orderItem.Product.FormatSku(orderItem.AttributesXml) : string.Empty, 50),

                    quantity = orderItem.Quantity
                };

                //force to use billing address as the destination one in the accordance with EU VAT rules (if enabled)
                var useEuVatRules = _taxSettings.EuVatEnabled
                    && (orderItem.Product?.IsTelecommunicationsOrBroadcastingOrElectronicServices ?? false)
                    && ((order.BillingAddress.Country
                        ?? _countryService.GetCountryById(order.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId))
                        ?? _countryService.GetCountryByTwoLetterIsoCode(_geoLookupService.LookupCountryIsoCode(order.Customer.LastIpAddress)))
                        ?.SubjectToVat ?? false)
                    && order.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId) != (int)VatNumberStatus.Valid;
                if (useEuVatRules)
                {
                    var destinationAddress = MapAddress(order.BillingAddress);
                    if (destinationAddress != null)
                        item.addresses = new AddressesModel { shipTo = destinationAddress };
                }

                //set tax code
                var productTaxCategory = _taxCategoryService.GetTaxCategoryById(orderItem.Product?.TaxCategoryId ?? 0);
                item.taxCode = CommonHelper.EnsureMaximumLength(productTaxCategory?.Name, 25);

                //whether entity use code is set
                var entityUseCode = orderItem.Product != null
                    ? orderItem.Product.GetAttribute<string>(AvalaraTaxDefaults.EntityUseCodeAttribute) : null;
                if (!string.IsNullOrEmpty(entityUseCode))
                    item.customerUsageType = CommonHelper.EnsureMaximumLength(entityUseCode, 25);

                return item;
            });
        }

        /// <summary>
        /// Create a separate item line for the order payment method additional fee
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Item line</returns>
        private LineItemModel CreateLineForPaymentMethod(Order order)
        {
            var paymentItem = new LineItemModel
            {
                amount = order.PaymentMethodAdditionalFeeExclTax,

                //item description
                description = "Payment method additional fee",

                //set payment method system name as item code
                itemCode = CommonHelper.EnsureMaximumLength(order.PaymentMethodSystemName, 50),

                quantity = 1
            };

            //whether payment is taxable
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                //try to get tax code
                var paymentTaxCategory = _taxCategoryService.GetTaxCategoryById(_taxSettings.PaymentMethodAdditionalFeeTaxClassId);
                paymentItem.taxCode = CommonHelper.EnsureMaximumLength(paymentTaxCategory?.Name, 25);
            }
            else
            {
                //if payment is non-taxable, set it as exempt
                paymentItem.exemptionCode = "Payment-fee-non-taxable";
            }

            return paymentItem;
        }

        /// <summary>
        /// Create a separate item line for the order shipping charge
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Item line</returns>
        private LineItemModel CreateLineForShipping(Order order)
        {
            var shippingItem = new LineItemModel
            {
                amount = order.OrderShippingExclTax,

                //item description
                description = "Shipping rate",

                //set shipping method name as item code
                itemCode = CommonHelper.EnsureMaximumLength(order.ShippingMethod, 50),

                quantity = 1
            };

            //whether shipping is taxable
            if (_taxSettings.ShippingIsTaxable)
            {
                //try to get tax code
                var shippingTaxCategory = _taxCategoryService.GetTaxCategoryById(_taxSettings.ShippingTaxClassId);
                shippingItem.taxCode = CommonHelper.EnsureMaximumLength(shippingTaxCategory?.Name, 25);
            }
            else
            {
                //if shipping is non-taxable, set it as exempt
                shippingItem.exemptionCode = "Shipping-rate-non-taxable";
            }

            return shippingItem;
        }

        /// <summary>
        /// Create item lines for order checkout attributes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Collection of item lines</returns>
        private IEnumerable<LineItemModel> CreateLinesForCheckoutAttributes(Order order)
        {
            //get checkout attributes values
            var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);

            return attributeValues.Where(attributeValue => attributeValue.CheckoutAttribute != null).Select(attributeValue =>
            {
                //create line
                var checkoutAttributeItem = new LineItemModel
                {
                    amount = attributeValue.PriceAdjustment,

                    //item description
                    description = CommonHelper.EnsureMaximumLength($"{attributeValue.CheckoutAttribute.Name} ({attributeValue.Name})", 2096),

                    //whether the discount to the item was applied
                    discounted = order.OrderSubTotalDiscountExclTax > decimal.Zero,

                    //set checkout attribute name and value as item code
                    itemCode = CommonHelper.EnsureMaximumLength($"{attributeValue.CheckoutAttribute.Name}-{attributeValue.Name}", 50),

                    quantity = 1
                };

                //whether checkout attribute is tax exempt
                if (attributeValue.CheckoutAttribute.IsTaxExempt)
                    checkoutAttributeItem.exemptionCode = "Attribute-non-taxable";
                else
                {
                    //or try to get tax code
                    var attributeTaxCategory = _taxCategoryService.GetTaxCategoryById(attributeValue.CheckoutAttribute.TaxCategoryId);
                    checkoutAttributeItem.taxCode = CommonHelper.EnsureMaximumLength(attributeTaxCategory?.Name, 25);
                }

                //whether entity use code is set
                var entityUseCode = attributeValue.CheckoutAttribute
                    .GetAttribute<string>(AvalaraTaxDefaults.EntityUseCodeAttribute);
                if (!string.IsNullOrEmpty(entityUseCode))
                    checkoutAttributeItem.customerUsageType = CommonHelper.EnsureMaximumLength(entityUseCode, 25);

                return checkoutAttributeItem;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            if (calculateTaxRequest.Address == null)
                return new CalculateTaxResult { Errors = new[] { "Address is not set" } };

            //construct a cache key
            var cacheKey = string.Format(AvalaraTaxDefaults.TaxRateCacheKey,
                calculateTaxRequest.Address.Address1,
                calculateTaxRequest.Address.City,
                calculateTaxRequest.Address.StateProvince?.Id ?? 0,
                calculateTaxRequest.Address.Country?.Id ?? 0,
                calculateTaxRequest.Address.ZipPostalCode);

            //we don't use standard way _cacheManager.Get() due the need write errors to CalculateTaxResult
            if (StaticCacheManager.IsSet(cacheKey))
            {
                var taxRate = StaticCacheManager.Get<decimal>(cacheKey);

                if (taxRate == -1)
                {
                    return new CalculateTaxResult { Errors = new[] { "No response from the service" }.ToList() };
                }

                return new CalculateTaxResult { TaxRate = taxRate };
            }

            //get estimated tax
            var totalTax = CreateEstimatedTaxTransaction(calculateTaxRequest.Address, calculateTaxRequest.Customer?.Id.ToString())?.totalTax;
            if (!totalTax.HasValue)
            {
                StaticCacheManager.Set(cacheKey, -1.0m, 60);
                return new CalculateTaxResult { Errors = new[] { "No response from the service" }.ToList() };
            }

            //tax rate successfully received, so cache it
            StaticCacheManager.Set(cacheKey, totalTax.Value, 60);

            return new CalculateTaxResult { TaxRate = totalTax.Value };
        }

        /// <summary>
        /// Create tax transaction to get estimated (not finally calculated) tax
        /// </summary>
        /// <param name="destinationAddress">Destination tax address</param>
        /// <param name="customerCode">Customer code</param>
        /// <returns>Transaction</returns>
        public TransactionModel CreateEstimatedTaxTransaction(Address destinationAddress, string customerCode)
        {
            var transactionModel = PrepareEstimatedTaxModel(destinationAddress, customerCode);
            return _avalaraTaxManager.CreateTaxTransaction(transactionModel, false);
        }

        /// <summary>
        /// Create tax transaction to get tax for the order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="save">Whether to save tax transaction</param>
        /// <returns>Transaction</returns>
        public TransactionModel CreateOrderTaxTransaction(Order order, bool save)
        {
            var transactionModel = PrepareOrderTaxModel(order, save);
            return _avalaraTaxManager.CreateTaxTransaction(transactionModel, save);
        }

        /// <summary>
        /// Void tax transaction
        /// </summary>
        /// <param name="order">Order</param>
        public void VoidTaxTransaction(Order order)
        {
            _avalaraTaxManager.VoidTax(new VoidTransactionModel
            {
                code = VoidReasonCode.DocVoided
            }, order.Id.ToString());
        }

        /// <summary>
        /// Delete tax transaction
        /// </summary>
        /// <param name="order">Order</param>
        public void DeleteTaxTransaction(Order order)
        {
            _avalaraTaxManager.VoidTax(new VoidTransactionModel
            {
                code = VoidReasonCode.DocDeleted
            }, order.Id.ToString());
        }

        /// <summary>
        /// Refund tax transaction
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        public void RefundTaxTransaction(Order order, decimal amountToRefund)
        {
            //first try to get saved tax transaction
            var transaction = _avalaraTaxManager.GetTransaction(order.Id.ToString());

            //create request parameters to refund transaction
            var refundTransaction = new RefundTransactionModel
            {
                referenceCode = CommonHelper.EnsureMaximumLength(transaction.code, 50),
                refundDate = transaction.date ?? DateTime.UtcNow,
                refundType = RefundType.Full
            };

            //whether refund is partial
            var isPartialRefund = amountToRefund < order.OrderTotal;
            if (isPartialRefund)
            {
                refundTransaction.refundType = RefundType.Percentage;
                refundTransaction.refundPercentage = amountToRefund / (order.OrderTotal - order.OrderTax) * 100;
            }

            _avalaraTaxManager.RefundTax(refundTransaction, transaction.code);
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
            controllerName = "AvalaraTaxAdmin";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Tax.Avalara.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                "admin_customer_details_info_top",
                "admin_customer_role_details_top",
                "admin_tax_settings_top",
                "admin_product_list_buttons",
                "admin_tax_category_list_buttons",
                "op_checkout_billing_address_top"
            };
        }

        //TODO: Move this logic to the new controllers
        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone.Equals("admin_customer_details_info_top") ||
                widgetZone.Equals("admin_customer_role_details_top"))
            {
                return AvalaraTaxDefaults.EntityUseCodeViewComponentName;
            }

            if (widgetZone.Equals("admin_tax_settings_top"))
                return AvalaraTaxDefaults.TaxOriginViewComponentName;

            if (widgetZone.Equals("admin_product_list_buttons"))
                return AvalaraTaxDefaults.ExportItemsViewComponentName;

            if (widgetZone.Equals("admin_tax_category_list_buttons"))
                return AvalaraTaxDefaults.TaxCodesViewComponentName;

            return null;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            _objectContext.Install();

            //settings
            _settingService.SaveSetting(new AvalaraTaxSettings
            {
                CompanyCode = Guid.Empty.ToString(),
                UseSandbox = true,
                CommitTransactions = true,
                TaxOriginAddressType = TaxOriginAddressType.ShippingOrigin
            });

            //locales
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Create", "Create request");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.CreateResponse", "Create response");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Error", "Error");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Refund", "Refund request");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.RefundResponse", "Refund response");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Void", "Void request");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.VoidResponse", "Void response");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.TaxOriginAddressType.DefaultTaxAddress", "Default tax address");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.TaxOriginAddressType.ShippingOrigin", "Shipping origin address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ValidatingAddressErrorDescription", "Our system could not validate the address provided:");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ValidatingAddressErrorDescription2", "If the address you entered is correct, please proceed with the checkout. If you need assistance please contact the office at 1-800-213-2401.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.Confirm", "We were not able to verify the address entered but found a similar one. Please review the recommended address below:");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.GenericErrorMessage", "Please contact the office at 1 - 800 - 213 - 2401 as there has been an internal error with our system.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches", "Multiple matches for the address are all in the same ZIP/Postal Code and carrier route. No +4 information is available.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches2", "There is not enough information available in the input address to break the tie between multiple matching records.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches3", "Either the directionals or the suffix field did not match the post office database, and there was more than one choice for correcting the address.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatchesResponse", "There were multiple matches for the address provided. Please ensure that your full address is entered.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases", "Address Validation for this country not supported.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases2", "Address geocoding for this country not supported.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchasesResponse", "Please contact the office at 1-800-213-2401 to discuss options for international purchases.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetName", "An exact street name match could not be found and phonetically matching the street name resulted in either no matches or matches to more than one street name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNameResponse", "Please ensure that the street address entered is correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumber", "The address was found but the street number in the input address was not between the low and high range of the post office database.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumberResponse", "Please ensure that the street number is correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorCity", "The city could not be found or determined from postal code.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorCityResponse", "Please ensure that the City and ZIP code entered are correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorZip", "Please ensure that the ZIP Code entered is correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorZipResponse", "Please ensure that the ZIP Code entered is correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddress", "This is a new address that will not validate properly until the next database update.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddressResponse", "If this is a new address, please proceed with your order. We will contact you if there are any issues.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverable", "The physical location exists but there are no homes on this street. One reason might be railroad tracks or rivers running alongside this street, as they would prevent construction of homes in this location.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverableResponse", "The address entered exists but is not available for delivery. Please ensure that the address entered is correct.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.Error", "For the correct tax calculation we need the most accurate address. There are some errors from the validation system: {0}");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.PopupTitle", "Verify Your Shipping Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.PopupTitleValidationError", "Address Verification Error");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.UseEnteredAddress", "Use The Address I Entered");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.UseValidatedAddress", "Use This Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Configuration", "Configuration");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.AccountId", "Account ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.AccountId.Hint", "Specify Avalara account ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.CommitTransactions", "Commit transactions");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.CommitTransactions.Hint", "Determine whether to commit tax transactions right after they are saved.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company", "Company");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.Currency.Warning", "The default currency used by _localizationService company does not match the primary store currency");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.Hint", "Choose a company that was previously added to the Avalara account.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.NotExist", "There are no active companies");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode", "Entity use code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode.Hint", "Choose a code that can be used to designate the reason for a particular sale being exempt. Each entity use code stands for a different exemption reason, the logic of which can be found in Avalara exemption reason documentation.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode.None", "None");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.LicenseKey", "License key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.LicenseKey.Hint", "Specify Avalara account license key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxCodeDescription", "Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxCodeType", "Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxOriginAddressType", "Tax origin address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxOriginAddressType.Hint", "Choose which address will be used as the origin for tax requests to Avalara services.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.UseSandbox", "Use sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.UseSandbox.Hint", "Determine whether to use sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.ValidateAddress", "Validate address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Fields.ValidateAddress.Hint", "Determine whether to validate entered by customer addresses before the tax calculation.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Items.Export", "Export to Avalara (selected)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.AlreadyExported", "Selected products have already been exported");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.Error", "An error has occurred on export products");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.Success", "Successfully exported {0} products");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log", "Log");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.BackToList", "back to log");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.ClearLog", "Clear log");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.CreatedDate", "Created on");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.CreatedDate.Hint", "Date and time the log entry was created.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Customer", "Customer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Customer.Hint", "Name of the customer.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Deleted", "The log entry has been deleted successfully.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Hint", "View log entry details");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.RequestMessage", "Request message");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.RequestMessage.Hint", "The details of the request.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.ResponseMessage", "Response message");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.ResponseMessage.Hint", "The details of the response.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.StatusCode", "Status code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.StatusCode.Hint", "The status code of the response.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Url", "Url");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Url.Hint", "The requested URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedFrom", "Created from");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedFrom.Hint", "The creation from date for the search.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedTo", "Created to");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedTo.Hint", "The creation to date for the search.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes", "Avalara tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete", "Delete Avalara system tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete.Error", "An error has occurred on delete tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete.Success", "System tax codes successfully deleted");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export", "Export tax codes to Avalara");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.AlreadyExported", "All tax codes have already been exported");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.Error", "An error has occurred on export tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.Success", "Successfully exported {0} tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import", "Import Avalara system tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import.Error", "An error has occurred on import tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import.Success", "Successfully imported {0} tax codes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TestTax", "Test tax calculation");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TestTax.Error", "An error has occurred on tax request");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.TestTax.Success", "The tax was successfully received");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials", "Test connection");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials.Declined", "Credentials declined");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials.Verified", "Credentials verified");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //database objects
            _objectContext.Uninstall();

            //generic attributes
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
            {
                _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeDescriptionAttribute, null);
                _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, null);
            }
            foreach (var customer in _customerService.GetAllCustomers())
            {
                _genericAttributeService.SaveAttribute<string>(customer, AvalaraTaxDefaults.EntityUseCodeAttribute, null);
            }
            foreach (var customerRole in _customerService.GetAllCustomerRoles(true))
            {
                _genericAttributeService.SaveAttribute<string>(customerRole, AvalaraTaxDefaults.EntityUseCodeAttribute, null);
            }
            foreach (var product in _productService.SearchProducts(showHidden: true))
            {
                _genericAttributeService.SaveAttribute<string>(product, AvalaraTaxDefaults.EntityUseCodeAttribute, null);
            }
            foreach (var attribute in _checkoutAttributeService.GetAllCheckoutAttributes())
            {
                _genericAttributeService.SaveAttribute<string>(attribute, AvalaraTaxDefaults.EntityUseCodeAttribute, null);
            }

            //settings            
            _taxSettings.ActiveTaxProviderSystemName = _taxService.LoadAllTaxProviders()
                .FirstOrDefault(taxProvider => !taxProvider.PluginDescriptor.SystemName.Equals(AvalaraTaxDefaults.SystemName))
                ?.PluginDescriptor.SystemName;
            _settingService.SaveSetting(_taxSettings);
            _widgetSettings.ActiveWidgetSystemNames.Remove(AvalaraTaxDefaults.SystemName);
            _settingService.SaveSetting(_widgetSettings);
            _settingService.DeleteSetting<AvalaraTaxSettings>();

            //locales
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Create");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.CreateResponse");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Error");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Refund");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.RefundResponse");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.Void");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.LogType.VoidResponse");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.TaxOriginAddressType.DefaultTaxAddress");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Tax.Avalara.Domain.TaxOriginAddressType.ShippingOrigin");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ValidatingAddressErrorDescription");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ValidatingAddressErrorDescription2");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.Confirm");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.GenericErrorMessage");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches2");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatches3");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorMultipleMatchesResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchases2");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorInternationalPurchasesResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetName");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNameResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumber");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorStreetNumberResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorCity");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorCityResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorZip");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorZipResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddress");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNewAddressResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverable");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.ErrorNotDeliverableResponse");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Configuration");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.AccountId");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.AccountId.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.CommitTransactions");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.CommitTransactions.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.Currency.Warning");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.Company.NotExist");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.EntityUseCode.None");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.LicenseKey");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.LicenseKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxCodeDescription");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxCodeType");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxOriginAddressType");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.TaxOriginAddressType.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.ValidateAddress");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Fields.ValidateAddress.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Items.Export");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.AlreadyExported");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Items.Export.Success");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.BackToList");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.ClearLog");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.CreatedDate");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.CreatedDate.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Customer");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Customer.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Deleted");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Message");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Message.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.RequestMessage");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.RequestMessage.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.ResponseMessage");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.ResponseMessage.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.StatusCode");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.StatusCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Url");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Url.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedFrom");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedFrom.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedTo");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.Log.Search.CreatedTo.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Delete.Success");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.AlreadyExported");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Export.Success");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TaxCodes.Import.Success");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TestTax");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TestTax.Error");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.TestTax.Success");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials.Declined");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.VerifyCredentials.Verified");
            this.DeletePluginLocaleResource("Plugins.Tax.Avalara.AddressValidation.PopupTitleValidationError");
            
            base.Uninstall();
        }

        #endregion
    }
}