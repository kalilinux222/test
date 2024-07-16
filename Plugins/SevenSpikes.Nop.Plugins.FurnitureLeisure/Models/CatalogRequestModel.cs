using FluentValidation.Attributes;
using Nop.Web.Framework;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    [Validator(typeof(CatalogRequestModelValidator))]
    public class CatalogRequestModel
    {
        public CatalogRequestModel()
        {
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.CompanyName")]
        public string CompanyName { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.StreetAddress")]
        public string StreetAddress { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.City")]
        public string City { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.StateId")]
        public int StateId { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.ZipCode")]
        public string ZipCode { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.IsUsefullInformation")]
        public int CreateAccount { get; set; }

        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.ShouldRecieveCall")]
        public int ShouldRecieveCall { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("SevenSpikes.FurnitureLeisure.Public.CatalogRequest.ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }
    }
}
