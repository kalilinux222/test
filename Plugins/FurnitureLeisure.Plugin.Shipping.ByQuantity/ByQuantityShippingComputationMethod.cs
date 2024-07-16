namespace FurnitureLeisure.Plugin.Shipping.ByQuantity
{
    using System;
    using System.Linq;
    using System.Web.Routing;
    using Nop.Core;
    using Nop.Core.Domain.Shipping;
    using Nop.Core.Plugins;
    using Nop.Services.Localization;
    using Nop.Services.Shipping;
    using Nop.Services.Shipping.Tracking;

    public class ByQuantityShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;

        public ByQuantityShippingComputationMethod(IShippingService shippingService,
            IStoreContext storeContext)
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
        }

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="ship1"></param>
        /// <param name="ship10"></param>
        /// <remarks>
        /// Each Item Variant has a Ship1 price and a Ship10 price. THESE ARE REQUIRED FIELDS for each product.
        /// ratio=(Ship10-Ship1)/9
        /// cost=Ship1+((Qnty-1)*ratio)
        /// </remarks>
        /// <returns></returns>
        private decimal? GetRate(int quantity, decimal? ship1, decimal? ship10)
        {
            if (!ship1.HasValue || !ship10.HasValue)
            {
                return null;
            }

            decimal ratio = (ship10.Value - ship1.Value) / 9;
            decimal shippingTotal = ship1.Value + (quantity - 1) * ratio;

            return shippingTotal;
        }

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }


            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;
            decimal? rate = decimal.Zero;
            foreach (var packageItem in getShippingOptionRequest.Items)
            {
                if (packageItem.ShoppingCartItem.IsFreeShipping)
                {
                    continue;
                }

                decimal? ship1 = packageItem.ShoppingCartItem.Product.Ship1;
                decimal? ship10 = packageItem.ShoppingCartItem.Product.Ship10;

                // Added by Nop-Templates.com
                var productAttributeMappings = packageItem.ShoppingCartItem.Product.ProductAttributeMappings;

                foreach (var productAttributeMapping in productAttributeMappings)
                {
                    var firstProductAttributeValueWithShip1AndShip10 = productAttributeMapping.ProductAttributeValues.FirstOrDefault(pav => pav.Ship1 != decimal.Zero && pav.Ship10 != decimal.Zero);

                    if (firstProductAttributeValueWithShip1AndShip10 != null)
                    {
                        ship1 = firstProductAttributeValueWithShip1AndShip10.Ship1;
                        ship10 = firstProductAttributeValueWithShip1AndShip10.Ship10;

                        break;
                    }
                }

                rate += GetRate(packageItem.GetQuantity(), ship1, ship10);
            }

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                    shippingOption.Rate = rate.Value;
                    response.ShippingOptions.Add(shippingOption);
                }
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
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
            controllerName = "ShippingByWeight";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.ByWeight.Controllers" }, { "area", null } };
        }

    }
}
