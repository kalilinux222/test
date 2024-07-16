using Nop.Web.Framework;
using System.ComponentModel.DataAnnotations;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Models
{
    public class CarouselImageModel
    {
        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}