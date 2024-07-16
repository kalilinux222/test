using Nop.Admin.Controllers;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.ModelMappings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin
{
    public class FurnitureLeisureProductTagsImportController : BaseAdminController
	{
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IFurnitureLeisureExportManager _exportManager;
        private readonly IStoreService _storeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IFurnitureLeisureImportManager _importManager;

		public FurnitureLeisureProductTagsImportController(IPermissionService permissionService,
            IWorkContext workContext,
            IProductService productService,
            IProductTemplateService productTemplateService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IFurnitureLeisureExportManager exportManager,
            IStoreService storeService,
            IManufacturerService manufacturerService,
            IVendorService vendorService,
            IShippingService shippingService,
            IFurnitureLeisureImportManager importManager)
		{
            _permissionService = permissionService;
            _workContext = workContext;
            _productService = productService;
            _productTemplateService = productTemplateService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _exportManager = exportManager;
            _storeService = storeService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _shippingService = shippingService;
            _importManager = importManager;

        }

		public ActionResult ProductTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            var model = new ProductListModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });
            }

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
            {
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var wh in _shippingService.GetAllWarehouses())
            {
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });
            }

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
            {
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
            }

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            {
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));
            }

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
            {
                overridePublished = true;
            }
            else if (model.SearchPublishedId == 2)
            {
                overridePublished = false;
            }

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: overridePublished
            );

            var productTemplates = _productTemplateService.GetAllProductTemplates();

            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                //little hack here:
                //ensure that product full descriptions are not returned
                //otherwise, we can get the following error if products have too long descriptions:
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                productModel.FullDescription = "";

                var productTemplate = productTemplates.FirstOrDefault(pt => pt.Id == x.ProductTemplateId);

                if (productTemplate != null)
                {
                    productModel.CustomProperties["TemplateName"] = productTemplate.Name;
                }

                //picture
                var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);
                //product type
                productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                //friendly stock qantity
                //if a simple product AND "manage inventory" is "Track inventory", then display
                if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();
                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost, ActionName("ProductTags")]
        [FormValueRequired("exportcsv-all")]
        public ActionResult ExportCsvAll(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            {
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));
            }

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
            {
                overridePublished = true;
            }
            else if (model.SearchPublishedId == 2)
            {
                overridePublished = false;
            }

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            );

            var models = products.Select(x => x.ToImportModel()).ToList();

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _exportManager.ExportToCsv(stream, models);
                    bytes = stream.ToArray();
                }
                return File(bytes, "text", "product_productTags_mappings.csv");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("ProductTags");
            }
        }

        [HttpPost]
        public ActionResult ExportCsvSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var models = products.Select(x => x.ToImportModel()).ToList();

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _exportManager.ExportToCsv(stream, models);
                    bytes = stream.ToArray();
                }
                return File(bytes, "text", "product_productTags_mappings.csv");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("ProductTags");
            }
        }

        [HttpPost]
        public ActionResult ImportCsv()
        {
            return ImportTags(false);
        }

        private ActionResult ImportTags(bool importAll)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            //a vendor cannot import products
            if (_workContext.CurrentVendor != null)
            {
                return AccessDeniedView();
            }

            try
            {
                var fileName = importAll ? "importcsvallfile" : "importcsvfile";
                var file = Request.Files[fileName];
                if (file != null && file.ContentLength > 0)
                {
                    if (importAll)
                    {
                        _importManager.ImportCsvAll(file.InputStream);
                    }
                    else
                    {
                        _importManager.ImportCsv(file.InputStream);
                    }
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("ProductTags");
                }
                SuccessNotification(_localizationService.GetResource("SevenSpikes.FurnitureLeisure.Admin.ProductTagsImport.ProductTagsImported"));
                return RedirectToAction("ProductTags");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("ProductTags");
            }
        }

        [NonAction]
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }
    }
}
