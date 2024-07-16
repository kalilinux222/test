using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.Avalara.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Web;

namespace Nop.Plugin.Tax.Avalara.Services
{
    /// <summary>
    /// Represents overridden order total calculation service
    /// </summary>
    public class OverriddenOrderTotalCalculationService : OrderTotalCalculationService
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IGiftCardService _giftCardService;
        private readonly IRewardPointService _rewardPointService;
        private readonly HttpContextBase _httpContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

        #region Ctor

        public OverriddenOrderTotalCalculationService(IWorkContext workContext, IStoreContext storeContext, IPriceCalculationService priceCalculationService, ITaxService taxService, IShippingService shippingService, IPaymentService paymentService, ICheckoutAttributeParser checkoutAttributeParser, IDiscountService discountService, IGiftCardService giftCardService, IGenericAttributeService genericAttributeService, IRewardPointService rewardPointService, TaxSettings taxSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, ShoppingCartSettings shoppingCartSettings, CatalogSettings catalogSettings, HttpContextBase httpContext) : base(workContext, storeContext, priceCalculationService, taxService, shippingService, paymentService, checkoutAttributeParser, discountService, giftCardService, genericAttributeService, rewardPointService, taxSettings, rewardPointsSettings, shippingSettings, shoppingCartSettings, catalogSettings)
        {
            _genericAttributeService = genericAttributeService;
            _rewardPointService = rewardPointService;
            _rewardPointsSettings = rewardPointsSettings;
            _paymentService = paymentService;
            _giftCardService = giftCardService;
            _priceCalculationService = priceCalculationService;
            _storeContext = storeContext;
            _taxService = taxService;
            _shoppingCartSettings = shoppingCartSettings;
            _httpContext = httpContext;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="appliedGiftCards">Applied gift cards</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="redeemedRewardPoints">Reward points to redeem</param>
        /// <param name="redeemedRewardPointsAmount">Reward points amount in primary store currency to redeem</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
        public override decimal? GetShoppingCartTotal(IList<ShoppingCartItem> cart,
            out decimal discountAmount, out Discount appliedDiscount,
            out List<AppliedGiftCard> appliedGiftCards,
            out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount,
            bool ignoreRewardPonts = false, bool usePaymentMethodAdditionalFee = true)
        {
            redeemedRewardPoints = 0;
            redeemedRewardPointsAmount = decimal.Zero;

            var customer = cart.GetCustomer();
            string paymentMethodSystemName = "";
            if (customer != null)
            {
                paymentMethodSystemName = customer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _genericAttributeService,
                    _storeContext.CurrentStore.Id);
            }


            //subtotal without tax
            decimal orderSubTotalDiscountAmount;
            Discount orderSubTotalAppliedDiscount;
            decimal subTotalWithoutDiscountBase;
            decimal subTotalWithDiscountBase;
            GetShoppingCartSubTotal(cart, false,
                out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            //subtotal with discount
            decimal subtotalBase = subTotalWithDiscountBase;



            //shipping without tax
            decimal? shoppingCartShipping = GetShoppingCartShippingTotal(cart, false);



            //payment method additional fee without tax
            decimal paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !String.IsNullOrEmpty(paymentMethodSystemName))
            {
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax =
                    _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee,
                        false, customer);
            }




            //tax
            decimal shoppingCartTax = GetTaxTotal(cart, usePaymentMethodAdditionalFee);

            //Avalara plugin changes
            //adjust tax total according to received value from the Avalara
            //TODO: CHeck this!
            shoppingCartTax = (_httpContext.Session[AvalaraTaxDefaults.TaxDetailsSessionValue] as TaxDetails)?.TaxTotal ?? shoppingCartTax;
            //Avalara plugin changes



            //order total
            decimal resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }
            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp);

            #region Order total discount

            discountAmount = GetOrderTotalDiscount(customer, resultTemp, out appliedDiscount);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp);

            #endregion

            #region Applied gift cards

            //let's apply gift cards now (gift cards that can be used)
            appliedGiftCards = new List<AppliedGiftCard>();
            if (!cart.IsRecurring())
            {
                //we don't apply gift cards for recurring products
                var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer);
                if (giftCards != null)
                    foreach (var gc in giftCards)
                        if (resultTemp > decimal.Zero)
                        {
                            decimal remainingAmount = gc.GetGiftCardRemainingAmount();
                            decimal amountCanBeUsed = resultTemp > remainingAmount ?
                                remainingAmount :
                                resultTemp;

                            //reduce subtotal
                            resultTemp -= amountCanBeUsed;

                            var appliedGiftCard = new AppliedGiftCard();
                            appliedGiftCard.GiftCard = gc;
                            appliedGiftCard.AmountCanBeUsed = amountCanBeUsed;
                            appliedGiftCards.Add(appliedGiftCard);
                        }
            }

            #endregion

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return null;
            }

            decimal orderTotal = resultTemp;

            #region Reward points

            if (_rewardPointsSettings.Enabled &&
                !ignoreRewardPonts &&
                customer.GetAttribute<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout,
                    _genericAttributeService, _storeContext.CurrentStore.Id))
            {
                int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(customer.Id, _storeContext.CurrentStore.Id);
                if (CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                {
                    decimal rewardPointsBalanceAmount = ConvertRewardPointsToAmount(rewardPointsBalance);
                    if (orderTotal > decimal.Zero)
                    {
                        if (orderTotal > rewardPointsBalanceAmount)
                        {
                            redeemedRewardPoints = rewardPointsBalance;
                            redeemedRewardPointsAmount = rewardPointsBalanceAmount;
                        }
                        else
                        {
                            redeemedRewardPointsAmount = orderTotal;
                            redeemedRewardPoints = ConvertAmountToRewardPoints(redeemedRewardPointsAmount);
                        }
                    }
                }
            }

            #endregion

            orderTotal = orderTotal - redeemedRewardPointsAmount;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                orderTotal = RoundingHelper.RoundPrice(orderTotal);
            return orderTotal;
        }

        #endregion
    }
}