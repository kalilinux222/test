using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.Avalara.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Tax.Avalara.Factories
{
    /// <summary>
    /// Represents overridden shopping cart model factory
    /// </summary>
    public class OverriddenShoppingCartController : ShoppingCartController
    {
        private readonly HttpContextBase _httpContext;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IGenericAttributeService _genericAttributeService;

        public OverriddenShoppingCartController(IProductService productService, IStoreContext storeContext, IWorkContext workContext, IShoppingCartService shoppingCartService, IPictureService pictureService, ILocalizationService localizationService, IProductAttributeService productAttributeService, IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser, ITaxService taxService, ICurrencyService currencyService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeFormatter checkoutAttributeFormatter, IOrderProcessingService orderProcessingService, IDiscountService discountService, ICustomerService customerService, IGiftCardService giftCardService, ICountryService countryService, IStateProvinceService stateProvinceService, IShippingService shippingService, IOrderTotalCalculationService orderTotalCalculationService, ICheckoutAttributeService checkoutAttributeService, IPaymentService paymentService, IWorkflowMessageService workflowMessageService, IPermissionService permissionService, IDownloadService downloadService, ICacheManager cacheManager, IWebHelper webHelper, ICustomerActivityService customerActivityService, IGenericAttributeService genericAttributeService, IAddressAttributeFormatter addressAttributeFormatter, HttpContextBase httpContext, MediaSettings mediaSettings, ShoppingCartSettings shoppingCartSettings, CatalogSettings catalogSettings, OrderSettings orderSettings, ShippingSettings shippingSettings, TaxSettings taxSettings, CaptchaSettings captchaSettings, AddressSettings addressSettings, RewardPointsSettings rewardPointsSettings) : base(productService, storeContext, workContext, shoppingCartService, pictureService, localizationService, productAttributeService, productAttributeFormatter, productAttributeParser, taxService, currencyService, priceCalculationService, priceFormatter, checkoutAttributeParser, checkoutAttributeFormatter, orderProcessingService, discountService, customerService, giftCardService, countryService, stateProvinceService, shippingService, orderTotalCalculationService, checkoutAttributeService, paymentService, workflowMessageService, permissionService, downloadService, cacheManager, webHelper, customerActivityService, genericAttributeService, addressAttributeFormatter, httpContext, mediaSettings, shoppingCartSettings, catalogSettings, orderSettings, shippingSettings, taxSettings, captchaSettings, addressSettings, rewardPointsSettings)
        {
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _httpContext = httpContext;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
        }

        /// <summary>
        /// Prepare tax details by Avalara tax service
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        private void PrepareTaxDetails(IList<ShoppingCartItem> cart)
        {
            //ensure that Avalara tax provider is active
            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider))
                return;

            //create dummy order for the tax request
            var order = new Order { Customer = _workContext.CurrentCustomer };

            //addresses
            order.BillingAddress = _workContext.CurrentCustomer.BillingAddress;
            order.ShippingAddress = _workContext.CurrentCustomer.ShippingAddress;

            //TODO: Test with PickUpInStore
            //if (_shippingSettings.AllowPickUpInStore)
            //{
            //    var pickupPoint = _workContext.CurrentCustomer.GetAttribute<PickupPoint>(SystemCustomerAttributeNames.SelectedPickupPointAttribute, _storeContext.CurrentStore.Id);
            //    if (pickupPoint != null)
            //    {
            //        var country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode);
            //        order.PickupAddress = new Address
            //        {
            //            Address1 = pickupPoint.Address,
            //            City = pickupPoint.City,
            //            Country = country,
            //            StateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(pickupPoint.StateAbbreviation, country?.Id),
            //            ZipPostalCode = pickupPoint.ZipPostalCode,
            //            CreatedOnUtc = DateTime.UtcNow,
            //        };
            //    }
            //}

            //checkout attributes
            order.CheckoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);

            //shipping method
            order.OrderShippingExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, false) ?? 0;
            order.ShippingMethod = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id)?.Name;

            //payment method
            var paymentMethod = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
            var paymentFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethod);
            order.PaymentMethodAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentFee, false, _workContext.CurrentCustomer);
            order.PaymentMethodSystemName = paymentMethod;

            //add discount amount
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, false, out var orderSubTotalDiscountExclTax, out _, out _, out _);
            order.OrderSubTotalDiscountExclTax = orderSubTotalDiscountExclTax;

            //create dummy order items
            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    AttributesXml = cartItem.AttributesXml,
                    Product = cartItem.Product,
                    Quantity = cartItem.Quantity
                };

                var itemSubtotal = _priceCalculationService.GetSubTotal(cartItem, true);
                orderItem.PriceExclTax = _taxService.GetProductPrice(cartItem.Product, itemSubtotal, false, _workContext.CurrentCustomer, out _);

                order.OrderItems.Add(orderItem);
            }

            //get tax details
            var taxTransaction = taxProvider.CreateOrderTaxTransaction(order, false);
            if (taxTransaction == null)
                return;

            //and save it for the further usage
            var taxDetails = new TaxDetails { TaxTotal = taxTransaction.totalTax };
            foreach (var item in taxTransaction.summary)
            {
                if (!item.rate.HasValue || !item.tax.HasValue)
                    continue;

                var taxRate = item.rate.Value * 100;
                var taxValue = item.tax.Value;

                if (!taxDetails.TaxRates.ContainsKey(taxRate))
                    taxDetails.TaxRates.Add(taxRate, taxValue);
                else
                    taxDetails.TaxRates[taxRate] = taxDetails.TaxRates[taxRate] + taxValue;
            }

            //TODO: Test this!
            _httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] = taxDetails;
        }


        #region Methods

        protected override OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Count > 0)
            {
                //Avalara plugin changes
                PrepareTaxDetails(cart);
                //Avalara plugin changes

                //subtotal
                decimal orderSubTotalDiscountAmountBase;
                Discount orderSubTotalAppliedDiscount;
                decimal subTotalWithoutDiscountBase;
                decimal subTotalWithDiscountBase;
                var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                    model.AllowRemovingSubTotalDiscount = orderSubTotalAppliedDiscount != null &&
                                                          orderSubTotalAppliedDiscount.RequiresCouponCode &&
                                                          !String.IsNullOrEmpty(orderSubTotalAppliedDiscount.CouponCode) &&
                                                          model.IsEditable;
                }

                //checkout attributes
                var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);

                model.CustomProperties["HasCheckoutAttributes"] = checkoutAttributesXml != null;

                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }

                //payment method fee
                var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {

                    SortedDictionary<decimal, decimal> taxRates;
                    decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);

                    //Avalara plugin changes
                    //get tax details from the Avalara tax service, it may slightly differ from the original calculated tax
                    //TODO: Test this!
                    var taxDetails = _httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] as TaxDetails;

                    if (taxDetails != null)
                    {
                        //adjust tax total according to received value from the Avalara
                        if (taxDetails.TaxTotal.HasValue)
                            shoppingCartTaxBase = taxDetails.TaxTotal.Value;

                        if (taxDetails.TaxRates?.Any() ?? false)
                            taxRates = new SortedDictionary<decimal, decimal>(taxDetails.TaxRates);
                    }
                    //Avalara plugin changes

                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), true, false),
                            });
                        }
                    }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //total
                decimal orderTotalDiscountAmountBase;
                Discount orderTotalAppliedDiscount;
                List<AppliedGiftCard> appliedGiftCards;
                int redeemedRewardPoints;
                decimal redeemedRewardPointsAmount;
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount,
                    out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                    model.AllowRemovingOrderTotalDiscount = orderTotalAppliedDiscount != null &&
                        orderTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderTotalAppliedDiscount.CouponCode) &&
                        model.IsEditable;
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Count > 0)
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled &&
                    _rewardPointsSettings.DisplayHowMuchWillBeEarned &&
                    shoppingCartTotalBase.HasValue)
                {
                    model.WillEarnRewardPoints = _orderTotalCalculationService
                        .CalculateRewardPoints(_workContext.CurrentCustomer, shoppingCartTotalBase.Value);
                }

            }

            return model;
        }

        #endregion
    }
}