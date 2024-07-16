using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Customers;
using Nop.Admin.Models.Orders;
using Nop.Admin.Models.Settings;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Plugin.Tax.Avalara.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework.Mvc;
using System;
using System.Web;

namespace Nop.Plugin.Tax.Avalara.Services
{
    /// <summary>
    /// Represents event consumer of Avalara tax provider
    /// </summary>
    public class EventConsumer :
        IConsumer<EntityDeleted<Order>>,
        IConsumer<OrderCancelledEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderRefundedEvent>,
        IConsumer<EntityUpdated<Order>>
    {
        #region Fields

        private readonly AvalaraTaxSettings _avalaraTaxSettings;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly HttpContextBase _httpContext;

        #endregion

        #region Ctor

        public EventConsumer(AvalaraTaxSettings avalaraTaxSettings,
            ICheckoutAttributeService checkoutAttributeService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            IProductService productService,
            ISettingService settingService,
            ITaxService taxService,
            IWorkContext workContext, HttpContextBase httpContext)
        {
            this._avalaraTaxSettings = avalaraTaxSettings;
            this._checkoutAttributeService = checkoutAttributeService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
            this._productService = productService;
            this._settingService = settingService;
            this._taxService = taxService;
            this._workContext = workContext;
            _httpContext = httpContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Save entity use code
        /// </summary>
        /// <param name="model">Base Nop entity model</param>
        private void SaveEntityUseCode(BaseNopEntityModel model)
        {
            //ensure that received model is BaseNopEntityModel
            if (model == null)
                return;

            //get entity by received model
            var entity = model is CustomerModel ? (BaseEntity)_customerService.GetCustomerById(model.Id)
                : model is CustomerRoleModel ? (BaseEntity)_customerService.GetCustomerRoleById(model.Id)
                : model is ProductModel ? (BaseEntity)_productService.GetProductById(model.Id)
                : model is CheckoutAttributeModel ? (BaseEntity)_checkoutAttributeService.GetCheckoutAttributeById(model.Id)
                : null;
            if (entity == null)
                return;

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return;

            //ensure that Avalara tax provider is active
            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
                return;

            // TODO: Check this!
            //whether there is a form value for the entity use code
            //if (_httpContext.Request.Form.TryGetValue(AvalaraTaxDefaults.EntityUseCodeAttribute, out StringValues entityUseCodeValue)
            //    && !StringValues.IsNullOrEmpty(entityUseCodeValue))
            //{
            //    //save attribute
            //    var entityUseCode = !entityUseCodeValue.ToString().Equals(Guid.Empty.ToString()) ? entityUseCodeValue.ToString() : null;
            //    _genericAttributeService.SaveAttribute(entity, AvalaraTaxDefaults.EntityUseCodeAttribute, entityUseCode);
            //}
        }

        /// <summary>
        /// Save tax origin address type
        /// </summary>
        /// <param name="model">Tax settings model</param>
        private void SaveTaxOriginAddressType(TaxSettingsModel model)
        {
            //ensure that received model is TaxSettingsModel
            if (model == null)
                return;

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return;

            //ensure that Avalara tax provider is active
            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
                return;

            // TODO: Check this!
            //whether there is a form value for the tax origin address type 
            //if (_httpContext.Request.Form.TryGetValue(AvalaraTaxDefaults.TaxOriginField, out StringValues taxOriginValue)
            //    && int.TryParse(taxOriginValue, out int taxOriginType))
            //{
            //    //save settings
            //    _avalaraTaxSettings.TaxOriginAddressType = (TaxOriginAddressType)taxOriginType;
            //    _settingService.SaveSetting(_avalaraTaxSettings);
            //}
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle order deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(EntityDeleted<Order> eventMessage)
        {
            if (eventMessage.Entity == null)
                return;

            //ensure that Avalara tax provider is active
            if (_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider)
            {
                //delete tax transaction
                taxProvider.DeleteTaxTransaction(eventMessage.Entity);
            }
        }

        // TODO: Check this!!!
        /// <summary>
        /// Handle model received event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        //public void HandleEvent(ModelReceivedEvent<BaseNopModel> eventMessage)
        //{
        //    SaveEntityUseCode(eventMessage?.Model as BaseNopEntityModel);
        //    SaveTaxOriginAddressType(eventMessage?.Model as TaxSettingsModel);
        //}

        /// <summary>
        /// Handle order cancelled event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(OrderCancelledEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            //ensure that Avalara tax provider is active
            if (_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider)
            {
                //void tax transaction
                taxProvider.VoidTaxTransaction(eventMessage.Order);
            }
        }

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            //ensure that Avalara tax provider is active
            if (_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider)
            {
                //create tax transaction
                taxProvider.CreateOrderTaxTransaction(eventMessage.Order, true);
            }
        }

        /// <summary>
        /// Handle order refunded event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(OrderRefundedEvent eventMessage)
        {
            if (eventMessage.Order == null)
                return;

            //ensure that Avalara tax provider is active
            if (_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider)
            {
                //refund tax transaction
                taxProvider.RefundTaxTransaction(eventMessage.Order, eventMessage.Amount);
            }
        }

        /// <summary>
        /// Handle order voided event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(EntityUpdated<Order> eventMessage)
        {
            if (eventMessage.Entity == null)
                return;

            //ensure that Avalara tax provider is active
            if (_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider taxProvider && eventMessage.Entity.PaymentStatus == PaymentStatus.Voided)
            {
                //void tax transaction
                taxProvider.VoidTaxTransaction(eventMessage.Entity);
            }
        }

        #endregion
    }
}