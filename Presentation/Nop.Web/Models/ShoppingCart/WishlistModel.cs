using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Validators.ShoppingCart;

namespace Nop.Web.Models.ShoppingCart
{
    
    public partial class WishlistModel : BaseNopModel
    {
        public WishlistModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            RequestAQuote = new RequestAQuoteModel();
        }

        public Guid CustomerGuid { get; set; }
        public string CustomerFullname { get; set; }

        public bool EmailWishlistEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public bool DisplayAddToCart { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public IList<ShoppingCartItemModel> Items { get; set; }

        public IList<string> Warnings { get; set; }

        //Added by Nop-Templates
        public RequestAQuoteModel RequestAQuote { get; set; }
        
		#region Nested Classes

        public partial class ShoppingCartItemModel : BaseNopEntityModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Warnings = new List<string>();
            }
            public string Sku { get; set; }

            public PictureModel Picture {get;set;}

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }
            
            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public IList<string> Warnings { get; set; }

        }

        #endregion

        #region Added by Nop-Templates

        [Validator(typeof(WishlistValidator))]
        public class RequestAQuoteModel
        {
            public RequestAQuoteModel()
            {
                Warnings = new List<string>();
            }

            [NopResourceDisplayName("RequestAQuote.Name")]
            public string Name { get; set; }

            [NopResourceDisplayName("RequestAQuote.Email")]
            public string Email { get; set; }

            [NopResourceDisplayName("RequestAQuote.Company")]
            public string Company { get; set; }

            [NopResourceDisplayName("RequestAQuote.StateProvince")]
            public string StateProvince { get; set; }

            [NopResourceDisplayName("RequestAQuote.City")]
            public string City { get; set; }

            [NopResourceDisplayName("RequestAQuote.DeliveryAddress")]
            public string DeliveryAddress { get; set; }

            [NopResourceDisplayName("RequestAQuote.DeliveryZip")]
            public string DeliveryZip { get; set; }

            [NopResourceDisplayName("RequestAQuote.PhoneNumber")]
            public string PhoneNumber { get; set; }

            public IList<string> Warnings { get; set; }

            public bool SuccessfullySent { get; set; }

            public string Result { get; set; }
        }

        #endregion
    }
}