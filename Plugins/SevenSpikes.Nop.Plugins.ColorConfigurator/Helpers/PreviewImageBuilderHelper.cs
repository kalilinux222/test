using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Media;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Helpers
{
    public class PreviewImageBuilderHelper : IPreviewImageBuilderHelper
    {
        private readonly ILogger _logger;
        private readonly IPictureService _pictureService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IWebHelper _webHelper;

        public PreviewImageBuilderHelper(
            ILogger logger, 
            IPictureService pictureService,
            IProductTemplateService productTemplateService, 
            IWebHelper webHelper)
        {
            _logger = logger;
            _pictureService = pictureService;
            _productTemplateService = productTemplateService;
            _webHelper = webHelper;
        }

        public string BuildImage(Product product, IList<ProductAttributeValue> attributeValues, int pictureSize = 0)
        {
            var basePicture = GetBasePictureBasedOnProductTemplate(product, ref attributeValues);

            if (basePicture == null)
            {
                return string.Empty;
            }

            string pictureName = BuildPictureName(attributeValues, basePicture.Id, pictureSize);

            var picturePath = GetPictureLocalPath(pictureName);

            // If picture already exists, return it
            if (File.Exists(picturePath))
            {
                return GetPictureUrl(pictureName);
            }

            var pictureBinary = _pictureService.LoadPictureBinary(basePicture);

            var memoryStream = new MemoryStream(pictureBinary);
            var baseImage = new Bitmap(Image.FromStream(memoryStream));

            var width = baseImage.Width;
            var height = baseImage.Height;

            if (pictureSize > 0)
            {
                width = pictureSize;
                height = width * baseImage.Height / baseImage.Width;
            }

            var finalImage = new Bitmap(width, height);

            var graphics = Graphics.FromImage(finalImage);

            graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(baseImage, 0, 0, width, height);

            foreach (var attributeValue in attributeValues)
            {
                var attributeValuePicture = _pictureService.GetPictureById(attributeValue.AttributeValuePictureId);

                if (attributeValuePicture == null)
                {
                    continue;
                }

                var attributeValuePictureBinary = _pictureService.LoadPictureBinary(attributeValuePicture);

                using (var attributeValueMemoryStream = new MemoryStream(attributeValuePictureBinary))
                {
                    var attributeValueBitMapImage = new Bitmap(Image.FromStream(attributeValueMemoryStream));

                    graphics.DrawImage(attributeValueBitMapImage, 0, 0, width, height);
                }
            }

            var fullPictureInBytes = PictureToByteArray(finalImage);

            SaveImageOnFileSystem(pictureName, fullPictureInBytes);

            return GetPictureUrl(pictureName);
        }

        private Picture GetBasePictureBasedOnProductTemplate(Product product, ref IList<ProductAttributeValue> attributeValues)
        {
            var productTemplate = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);

            if (productTemplate == null)
            {
                return null;
            }

            switch (productTemplate.ViewPath)
            {
                case Plugin.NonConditionalProductTemplate:
                {
                    var productImages = _pictureService.GetPicturesByProductId(product.Id);

                    // Get the first image and use it as a base image.
                    var productImage = productImages.FirstOrDefault();

                    if (productImage != null)
                    {
                        return productImage;
                    }

                    break;
                }
                case Plugin.ConditionalProductTemplate:
                {
                    var firstSelectedAttributeValue = attributeValues.FirstOrDefault(x => x.AttributeValuePictureId > 0);

                    if (firstSelectedAttributeValue != null)
                    {
                        var baseAttributePicture = _pictureService.GetPictureById(firstSelectedAttributeValue.AttributeValuePictureId);

                        if (baseAttributePicture != null)
                        {
                            // Remove the attribute from the collection as it will be used only as a base image.
                            attributeValues.Remove(firstSelectedAttributeValue);

                            return baseAttributePicture;
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private string BuildPictureName(IList<ProductAttributeValue> attributeValues, int basePictureId, int pictureSize)
        {
            string pictureName = $"{basePictureId:0000000}";

            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValuePictureId > 0)
                {
                    pictureName += $"_{attributeValue.AttributeValuePictureId}";
                }
            }

            pictureName += $"_{pictureSize}.png";

            return pictureName;
        }

        private void SaveImageOnFileSystem(string pictureName, byte[] pictureBinary)
        {
            string fileStoragePath = _webHelper.MapPath(Plugin.ImagesVirtualPath);

            if (!Directory.Exists(fileStoragePath))
            {
                Directory.CreateDirectory(fileStoragePath);
            }

            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, fileStoragePath);
            permissionSet.AddPermission(writePermission);

            if (!permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                return;
            }

            string filePath = Path.Combine(fileStoragePath, pictureName);

            if (!File.Exists(filePath))
            {
                try
                {
                    File.WriteAllBytes(filePath, pictureBinary);
                }
                catch
                {
                    _logger.Error("Preview Image for product failed to save");
                }
            }
        }

        private byte[] PictureToByteArray(Image picture)
        {
            using (var ms = new MemoryStream())
            {
                picture.Save(ms, ImageFormat.Png);

                return ms.ToArray();
            }
        }

        private string GetPictureUrl(string pictureName)
        {
            var url = _webHelper.GetStoreLocation() + Plugin.ImagesPath + pictureName;

            return url;
        }

        private string GetPictureLocalPath(string pictureName)
        {
            var directoryPath = _webHelper.MapPath(Plugin.ImagesVirtualPath);

            var filePath = Path.Combine(directoryPath, pictureName);

            return filePath;
        }
    }
}