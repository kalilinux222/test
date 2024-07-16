using SevenSpikes.Nop.Framework.Plugin;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants
{
    public class Plugin
    {
        public const string Name = "Nop Furniture Leisure";
        public const string ResourceName = "SevenSpikes.FurnitureLeisure";
        public const string SystemName = "SevenSpikes.Nop.Plugins.FurnitureLeisure";
        public const string FolderName = "SevenSpikes.Nop.Plugins.FurnitureLeisure";
        public const string UrlInStore = "http://www.nop-templates.com/furnitureleisure-plugin-for-nopcommerce";

        public const string ObjectContextName = "7spikesFurnitureLeisureContext";

        public const string DefaultAdminControllerNamespace = "SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin";

        public static bool IsTrialVersion
        {
            get
            {
#if TRIAL
                    return true;
#endif

                return false;
            }
        }

        public static List<MenuItem7Spikes> MenuItems => new List<MenuItem7Spikes>
        {
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.Settings", 
                SubMenuRelativePath = "FurnitureLeisureAdmin/Settings"
            },
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.CarouselImages",
                SubMenuRelativePath = "FurnitureLeisureAdmin/List"
            },
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.ProductTagsImport",
                SubMenuRelativePath = "furnitureLeisureproducttagsimport/producttags"
            },
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.ShopUnderSettings",
                SubMenuRelativePath = "ShopUnderAdmin/Settings"
            },
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.HomePageCategories",
                SubMenuRelativePath = "HomePageCategoriesAdmin/List"
            },
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.FurnitureLeisure.Admin.Submenus.PopularCategories",
                SubMenuRelativePath = "PopularCategoriesAdmin/List"
            }
        };

        public static string CatalogRequestMessageTemplateName = "FurnitureLeisure.CatalogRequestNotification";
        public static string CatalogRequestMessageTemplateSubject = "Catalog Request from %CatalogRequest.FirstName% %CatalogRequest.LastName%";
        public static string CatalogRequestMessageTemplateBody = "<p>First Name: %CatalogRequest.FirstName%</p><p> Last Name: %CatalogRequest.LastName%</p><p>Email: %CatalogRequest.Email%</p><p>Company/Business Name: %CatalogRequest.CompanyName%</p><p>Street Address: %CatalogRequest.StreetAddress%</p><p>City: %CatalogRequest.City%</p><p>State: %CatalogRequest.State%</p><p>Zip Code: %CatalogRequest.ZipCode%</p><p>Phone Number:&nbsp; %CatalogRequest.PhoneNumber%</p><p>Use this information to create a Furniture Leisure Account?: %CatalogRequest.CreateAccount%</p><p>Have a Furniture Leisure representative call you upon completion?: %CatalogRequest.ShouldRecieveCall%&nbsp;</p>";
    }
}