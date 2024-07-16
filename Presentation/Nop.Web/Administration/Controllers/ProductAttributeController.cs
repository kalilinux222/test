using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;

namespace Nop.Admin.Controllers
{
    public partial class ProductAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        //y.chan - custom
        private readonly ICopyProductAttributeService _copyProductAttributeService;

        #endregion Fields

        #region Constructors

        public ProductAttributeController(IProductService productService,
            IProductAttributeService productAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            ICopyProductAttributeService copyProductAttributeService)
        {
            this._productService = productService;
            this._productAttributeService = productAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            //y.chan - custom
            this._copyProductAttributeService = copyProductAttributeService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttribute,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(productAttribute,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdateLocales(PredefinedProductAttributeValue ppav, PredefinedProductAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(ppav,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        #endregion
        
        #region Methods

        #region Attribute list / create / edit / delete

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttributes = _productAttributeService
                .GetAllProductAttributes(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = productAttributes.Select(x => x.ToModel()),
                Total = productAttributes.TotalCount
            };

            return Json(gridModel);
        }
        
        //create
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var model = new ProductAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productAttribute = model.ToEntity();
                _productAttributeService.InsertProductAttribute(productAttribute);
                UpdateLocales(productAttribute, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewProductAttribute", _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = productAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            var model = productAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = productAttribute.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                productAttribute = model.ToEntity(productAttribute);
                _productAttributeService.UpdateProductAttribute(productAttribute);

                UpdateLocales(productAttribute, model);

                //activity log
                _customerActivityService.InsertActivity("EditProductAttribute", _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);
                
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = productAttribute.Id });
                }
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //y.chan - custom
        //apply predefined values accross all products that are using given attribute
        [HttpPost]
        public ActionResult Apply(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            _copyProductAttributeService.ApplyPredefinedAttributeValues(productAttribute);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.PredefinedValuesApplied"));

            //activity log
            _customerActivityService.InsertActivity("EditProductAttribute", _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);

            //selected tab
            SaveSelectedTabIndex();

            return RedirectToAction("Edit", new { id = productAttribute.Id });
        }

        [HttpPost]
        //do not validate request token (XSRF)
        [AdminAntiForgery(true)]
        public ActionResult AsyncUpload(int Id)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.UploadPictures))
            //    return Json(new { success = false, error = "You do not have required permissions" }, "text/plain");

            //we process it distinct ways based on a browser
            //find more info here http://stackoverflow.com/questions/4884920/mvc3-valums-ajax-file-upload
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                    throw new ArgumentException("No file uploaded");
                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();
            //contentType is not always available 
            //that's why we manually update it here
            //http://www.sfsu.edu/training/mimetype.htm
            if (String.IsNullOrEmpty(contentType))
            {
                switch (fileExtension)
                {
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".tiff":
                    case ".tif":
                        contentType = "image/tiff";
                        break;
                    default:
                        break;
                }
            }

            string filename = string.Format("palette_{0}.jpg", Id);
            string paletteImagePhy = string.Format("{0}{1}", Server.MapPath("~/Content/Images/palette/"), filename);

            System.IO.File.WriteAllBytes(paletteImagePhy, fileBinary);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                pictureId = Id,
                imageUrl = "/Content/Images/palette/"
            },
                "text/plain");
        }

        [HttpPost]
        //do not validate request token (XSRF)
        [AdminAntiForgery(true)]
        public ActionResult RemoveAttributePicture(string filePath)
        {
            FileInfo theFile = new FileInfo(filePath);
            if (theFile.Exists)
            {
                System.IO.File.Delete(filePath);
            } 
            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true
            },
                "text/plain");
        }
        
        //y.chan - custom end

        //delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                //No product attribute found with the specified id
                return RedirectToAction("List");

            _productAttributeService.DeleteProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductAttribute", _localizationService.GetResource("ActivityLog.DeleteProductAttribute"), productAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Used by products

        //used by products
        [HttpPost]
        public ActionResult UsedByProducts(DataSourceRequest command, int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var orders = _productService.GetProductsByProductAtributeId(
                productAttributeId: productAttributeId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    return new ProductAttributeModel.UsedByProductModel
                    {
                        Id = x.Id,
                        ProductName = x.Name,
                        Published = x.Published
                    };
                }),
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }
        
        #endregion

        #region Predefined values

        [HttpPost]
        public ActionResult PredefinedProductAttributeValueList(int productAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var values = _productAttributeService.GetPredefinedProductAttributeValues(productAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    return new PredefinedProductAttributeValueModel
                    {
                        Id = x.Id,
                        ProductAttributeId = x.ProductAttributeId,
                        Name = x.Name,
                        PriceAdjustment = x.PriceAdjustment,
                        PriceAdjustmentStr = x.PriceAdjustment.ToString("G29"),
                        WeightAdjustment = x.WeightAdjustment,
                        WeightAdjustmentStr = x.WeightAdjustment.ToString("G29"),
                        Cost = x.Cost,
                        IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder
                    };
                }),
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public ActionResult PredefinedProductAttributeValueCreatePopup(int productAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(productAttributeId);
            if (productAttribute == null)
                throw new ArgumentException("No product attribute found with the specified id");

            var model = new PredefinedProductAttributeValueModel();
            model.ProductAttributeId = productAttributeId;

            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public ActionResult PredefinedProductAttributeValueCreatePopup(string btnId, string formId, PredefinedProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.ProductAttributeId);
            if (productAttribute == null)
                throw new ArgumentException("No product attribute found with the specified id");

            if (ModelState.IsValid)
            {
                var ppav = new PredefinedProductAttributeValue
                {
                    ProductAttributeId = model.ProductAttributeId,
                    Name = model.Name,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    Cost = model.Cost,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };

                _productAttributeService.InsertPredefinedProductAttributeValue(ppav);
                UpdateLocales(ppav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult PredefinedProductAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var ppav = _productAttributeService.GetPredefinedProductAttributeValueById(id);
            if (ppav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            var model = new PredefinedProductAttributeValueModel
            {
                ProductAttributeId = ppav.ProductAttributeId,
                Name = ppav.Name,
                PriceAdjustment = ppav.PriceAdjustment,
                WeightAdjustment = ppav.WeightAdjustment,
                Cost = ppav.Cost,
                IsPreSelected = ppav.IsPreSelected,
                DisplayOrder = ppav.DisplayOrder
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = ppav.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost]
        public ActionResult PredefinedProductAttributeValueEditPopup(string btnId, string formId, PredefinedProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var ppav = _productAttributeService.GetPredefinedProductAttributeValueById(model.Id);
            if (ppav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            if (ModelState.IsValid)
            {
                ppav.Name = model.Name;
                ppav.PriceAdjustment = model.PriceAdjustment;
                ppav.WeightAdjustment = model.WeightAdjustment;
                ppav.Cost = model.Cost;
                ppav.IsPreSelected = model.IsPreSelected;
                ppav.DisplayOrder = model.DisplayOrder;
                _productAttributeService.UpdatePredefinedProductAttributeValue(ppav);

                UpdateLocales(ppav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public ActionResult PredefinedProductAttributeValueDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var ppav = _productAttributeService.GetPredefinedProductAttributeValueById(id);
            if (ppav == null)
                throw new ArgumentException("No predefined product attribute value found with the specified id");

            _productAttributeService.DeletePredefinedProductAttributeValue(ppav);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
