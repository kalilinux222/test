//using Avalara.AvaTax.RestClient;
//using Nop.Admin.Controllers;
//using Nop.Admin.Extensions;
//using Nop.Core;
//using Nop.Core.Caching;
//using Nop.Core.Domain.Cms;
//using Nop.Core.Domain.Tax;
//using Nop.Plugin.Tax.Avalara.Models.Tax;
//using Nop.Plugin.Tax.Avalara.Services;
//using Nop.Services.Cms;
//using Nop.Services.Common;
//using Nop.Services.Configuration;
//using Nop.Services.Localization;
//using Nop.Services.Security;
//using Nop.Services.Tax;
//using Nop.Web.Framework.Controllers;
//using Nop.Web.Framework.Kendoui;
//using Nop.Web.Framework.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Mvc;

//namespace Nop.Plugin.Tax.Avalara.Controllers
//{
//    public class OverriddenTaxController : TaxController
//    {
//        #region Fields

//        private readonly AvalaraTaxManager _avalaraTaxManager;
//        private readonly IGenericAttributeService _genericAttributeService;
//        private readonly ILocalizationService _localizationService;
//        private readonly IPermissionService _permissionService;
//        private readonly ISettingService _settingService;
//        private readonly ICacheManager _cacheManager;
//        private readonly ITaxCategoryService _taxCategoryService;
//        private readonly ITaxService _taxService;
//        private readonly IWidgetService _widgetService;
//        private readonly IWorkContext _workContext;
//        private readonly TaxSettings _taxSettings;
//        private readonly WidgetSettings _widgetSettings;

//        #endregion

//        #region Ctor
        
//        public OverriddenTaxController(ITaxService taxService, ITaxCategoryService taxCategoryService, TaxSettings taxSettings, ISettingService settingService, IPermissionService permissionService, AvalaraTaxManager avalaraTaxManager, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, ICacheManager cacheManager, IWidgetService widgetService, IWorkContext workContext, WidgetSettings widgetSettings) : base(taxService, taxCategoryService, taxSettings, settingService, permissionService)
//        {
//            _avalaraTaxManager = avalaraTaxManager;
//            _genericAttributeService = genericAttributeService;
//            _localizationService = localizationService;
//            _permissionService = permissionService;
//            _settingService = settingService;
//            _cacheManager = cacheManager;
//            _taxCategoryService = taxCategoryService;
//            _taxService = taxService;
//            _widgetService = widgetService;
//            _workContext = workContext;
//            _taxSettings = taxSettings;
//            _widgetSettings = widgetSettings;
//        }

//        #endregion

//        #region Methods

//        public override ActionResult Categories()
//        {
//            //ensure that Avalara tax provider is active
//            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//            {
//                //if isn't active return base action result
//                this.RouteData.Values["controller"] = "Tax";
//                return base.Categories();
//            }

//            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//                return AccessDeniedView();

//            //prepare available tax code types
//            var model = new DataSourceResult
//            {
//                Data = _avalaraTaxManager.GetTaxCodeTypes()?
//                    .Select(taxCodeType => new SelectListItem { Value = taxCodeType.Key, Text = taxCodeType.Value }).ToList()
//                    ?? new[] { new SelectListItem { Value = Guid.Empty.ToString(), Text = string.Empty } }.ToList()
//            };

//            //use overridden view
//            return View("~/Plugins/Tax.Avalara/Views/Tax/Categories.cshtml", model);
//        }

//        [HttpPost]
//        public override ActionResult Categories(DataSourceRequest command)
//        {
//            //ensure that Avalara tax provider is active
//            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//                return base.Categories(command);

//            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//                return AccessDeniedView();

//            //add pagination
//            var query = _taxCategoryService.GetAllTaxCategories().AsQueryable();
//            var taxCategories = new PagedList<TaxCategory>(query, command.Page - 1, command.PageSize);

//            //get tax types and define the default value
//            var taxTypes = _avalaraTaxManager.GetTaxCodeTypes()?.Select(taxType => new { Id = taxType.Key, Name = taxType.Value });
//            var defaultType = taxTypes
//                ?.FirstOrDefault(taxType => taxType.Name.Equals("Unknown", StringComparison.InvariantCultureIgnoreCase))
//                ?? taxTypes?.FirstOrDefault();

//            //prepare model
//            var categoriesModel = taxCategories.Select(taxCategory =>
//            {
//                var model = new TaxCategoryModel
//                {
//                    Id = taxCategory.Id,
//                    Name = taxCategory.Name,
//                    DisplayOrder = taxCategory.DisplayOrder
//                };

//                //try to get previously saved tax code type and description
//                var taxCodeType = taxTypes
//                    ?.FirstOrDefault(type => type.Id.Equals(taxCategory.GetAttribute<string>(AvalaraTaxDefaults.TaxCodeTypeAttribute) ?? string.Empty))
//                    ?? defaultType;
//                model.Type = taxCodeType?.Name ?? string.Empty;
//                model.TypeId = taxCodeType?.Id ?? Guid.Empty.ToString();
//                model.Description = taxCategory.GetAttribute<string>(AvalaraTaxDefaults.TaxCodeDescriptionAttribute) ?? string.Empty;

//                return model;
//            });

//            return Json(new DataSourceResult
//            {
//                Data = categoriesModel,
//                Total = taxCategories.TotalCount
//            });
//        }

//        //TODO: Check this. The method is missing in 3.7
//        //public ActionResult List()
//        //{
//        //    //ensure that Avalara tax provider is active
//        //    if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//        //    {
//        //        //if isn't active return base action result
//        //        this.RouteData.Values["controller"] = "Tax";
//        //        return base.List();
//        //    }

//        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//        //        return AccessDeniedView();

//        //    //prepare model
//        //    var model = _taxModelFactory.PrepareTaxConfigurationModel(new TaxConfigurationModel());

//        //    //prepare available tax code types
//        //    model.TaxCategories.CustomProperties.Add("TaxCodeTypes",
//        //        _cacheManager.Get(AvalaraTaxDefaults.TaxCodeTypesCacheKey, () => _avalaraTaxManager.GetTaxCodeTypes()));

//        //    //use overridden view
//        //    return View("~/Plugins/Tax.Avalara/Views/Tax/List.cshtml", model);
//        //}

//        [HttpPost]
//        public ActionResult TaxCategoryUpdate(Models.Tax.TaxCategoryModel model)
//        {
//            //ensure that Avalara tax provider is active
//            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//                return new NullJsonResult();

//            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//                return AccessDeniedView();

//            if (!ModelState.IsValid)
//                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

//            var taxCategory = _taxCategoryService.GetTaxCategoryById(model.Id);
//            taxCategory = model.ToEntity(taxCategory);
//            _taxCategoryService.UpdateTaxCategory(taxCategory);

//            //save tax code type as generic attribute
//            if (!string.IsNullOrEmpty(model.TypeId) && !model.TypeId.Equals(Guid.Empty.ToString()))
//                _genericAttributeService.SaveAttribute(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, model.TypeId);

//            return new NullJsonResult();
//        }

//        [HttpPost]
//        public ActionResult TaxCategoryAdd(Models.Tax.TaxCategoryModel model)
//        {
//            //ensure that Avalara tax provider is active
//            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//                return new NullJsonResult();

//            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//                return AccessDeniedView();

//            if (!ModelState.IsValid)
//                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });

//            var taxCategory = new TaxCategory();
//            taxCategory = model.ToEntity(taxCategory);
//            _taxCategoryService.InsertTaxCategory(taxCategory);

//            //save tax code type as generic attribute
//            if (!string.IsNullOrEmpty(model.TypeId) && !model.TypeId.Equals(Guid.Empty.ToString()))
//                _genericAttributeService.SaveAttribute(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, model.TypeId);

//            return new NullJsonResult();
//        }

//        [HttpPost]
//        public override ActionResult CategoryDelete(int id)
//        {
//            //ensure that Avalara tax provider is active
//            if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//                return new NullJsonResult();

//            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//                return AccessDeniedView();

//            var taxCategory = _taxCategoryService.GetTaxCategoryById(id);
//            if (taxCategory == null)
//                throw new ArgumentException("No tax category found with the specified id");

//            //delete generic attributes 
//            _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeDescriptionAttribute, null);
//            _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, null);

//            _taxCategoryService.DeleteTaxCategory(taxCategory);

//            return new NullJsonResult();
//        }

//        //TODO: List Method is missing in 3.7
//        //public ActionResult MarkAsPrimaryProvider(string systemName)
//        //{
//        //    if (string.IsNullOrEmpty(systemName))
//        //        return RedirectToAction("List", "Tax");

//        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//        //        return AccessDeniedView();

//        //    var taxProvider = _taxService.LoadTaxProviderBySystemName(systemName);
//        //    if (taxProvider != null)
//        //    {
//        //        //mark as primary provider
//        //        _taxSettings.ActiveTaxProviderSystemName = systemName;
//        //        _settingService.SaveSetting(_taxSettings);

//        //        //accordingly update widgets of Avalara tax provider
//        //        var avalaraWidgetIsActive = _widgetService.IsWidgetActive(_widgetService.LoadWidgetBySystemName(AvalaraTaxDefaults.SystemName));
//        //        var avalaraTaxProviderIsActive = _taxSettings.ActiveTaxProviderSystemName.Equals(AvalaraTaxDefaults.SystemName);
//        //        if (avalaraTaxProviderIsActive)
//        //        {
//        //            if (!avalaraWidgetIsActive)
//        //                _widgetSettings.ActiveWidgetSystemNames.Add(AvalaraTaxDefaults.SystemName);
//        //        }
//        //        else
//        //        {
//        //            if (avalaraWidgetIsActive)
//        //                _widgetSettings.ActiveWidgetSystemNames.Remove(AvalaraTaxDefaults.SystemName);
//        //        }
//        //        _settingService.SaveSetting(_widgetSettings);
//        //    }

//        //    return RedirectToAction("List", "Tax");
//        //}

//        //[HttpPost, ActionName("List")]
//        //[FormValueRequired("importTaxCodes")]
//        //public ActionResult ImportTaxCodes()
//        //{
//        //    //ensure that Avalara tax provider is active
//        //    if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//        //        return List();

//        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//        //        return AccessDeniedView();

//        //    //get Avalara pre-defined system tax codes (only active)
//        //    var systemTaxCodes = _avalaraTaxManager.GetSystemTaxCodes(true);
//        //    if (!systemTaxCodes?.Any() ?? true)
//        //    {
//        //        ErrorNotification(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Import.Error"));
//        //        return List();
//        //    }

//        //    //get existing tax categories
//        //    var existingTaxCategories = _taxCategoryService.GetAllTaxCategories().Select(taxCategory => taxCategory.Name).ToList();

//        //    //remove duplicates
//        //    var taxCodesToImport = systemTaxCodes.Where(taxCode => !existingTaxCategories.Contains(taxCode.taxCode)).ToList();

//        //    var importedTaxCodesNumber = 0;
//        //    foreach (var taxCode in taxCodesToImport)
//        //    {
//        //        if (string.IsNullOrEmpty(taxCode?.taxCode))
//        //            continue;

//        //        //create new tax category
//        //        var taxCategory = new TaxCategory { Name = taxCode.taxCode };
//        //        _taxCategoryService.InsertTaxCategory(taxCategory);

//        //        //save description and type
//        //        if (!string.IsNullOrEmpty(taxCode.description))
//        //            _genericAttributeService.SaveAttribute(taxCategory, AvalaraTaxDefaults.TaxCodeDescriptionAttribute, taxCode.description);
//        //        if (!string.IsNullOrEmpty(taxCode.taxCodeTypeId))
//        //            _genericAttributeService.SaveAttribute(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, taxCode.taxCodeTypeId);

//        //        importedTaxCodesNumber++;
//        //    }

//        //    //successfully imported
//        //    var successMessage = _localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Import.Success");
//        //    SuccessNotification(string.Format(successMessage, importedTaxCodesNumber));

//        //    return List();
//        //}

//        //[HttpPost, ActionName("List")]
//        //[FormValueRequired("exportTaxCodes")]
//        //public ActionResult ExportTaxCodes()
//        //{
//        //    //ensure that Avalara tax provider is active
//        //    if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//        //        return List();

//        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//        //        return AccessDeniedView();

//        //    //prepare tax codes to export
//        //    var taxCodesToExport = _taxCategoryService.GetAllTaxCategories().Select(taxCategory => new TaxCodeModel
//        //    {
//        //        createdDate = DateTime.UtcNow,
//        //        description = CommonHelper.EnsureMaximumLength(taxCategory.Name, 255),
//        //        isActive = true,
//        //        taxCode = CommonHelper.EnsureMaximumLength(taxCategory.Name, 25),
//        //        taxCodeTypeId = CommonHelper.EnsureMaximumLength(taxCategory
//        //            .GetAttribute<string>(AvalaraTaxDefaults.TaxCodeTypeAttribute) ?? "P", 2)
//        //    }).Where(taxCode => !string.IsNullOrEmpty(taxCode.taxCode)).ToList();

//        //    //get existing tax codes (only active)
//        //    var existingTaxCodes = _avalaraTaxManager.GetTaxCodes(true)?.Select(taxCode => taxCode.taxCode).ToList() ?? new List<string>();

//        //    //add Avalara pre-defined system tax codes
//        //    var systemTaxCodes = _avalaraTaxManager.GetSystemTaxCodes(true)?.Select(taxCode => taxCode.taxCode).ToList() ?? new List<string>();
//        //    existingTaxCodes.AddRange(systemTaxCodes);

//        //    //remove duplicates
//        //    taxCodesToExport = taxCodesToExport.Where(taxCode => !existingTaxCodes.Contains(taxCode.taxCode)).Distinct().ToList();

//        //    //export tax codes
//        //    if (taxCodesToExport.Any())
//        //    {
//        //        //create tax codes and get the result
//        //        var result = _avalaraTaxManager.CreateTaxCodes(taxCodesToExport)?.Count;

//        //        //display results
//        //        if (result.HasValue && result > 0)
//        //            SuccessNotification(string.Format(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Export.Success"), result));
//        //        else
//        //            ErrorNotification(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Export.Error"));
//        //    }
//        //    else
//        //        SuccessNotification(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Export.AlreadyExported"));

//        //    return List();
//        //}

//        //[HttpPost, ActionName("List")]
//        //[FormValueRequired("deleteTaxCodes")]
//        //public ActionResult DeleteSystemTaxCodes()
//        //{
//        //    //ensure that Avalara tax provider is active
//        //    if (!(_taxService.LoadActiveTaxProvider() is AvalaraTaxProvider))
//        //        return List();

//        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
//        //        return AccessDeniedView();

//        //    //get Avalara pre-defined system tax codes (only active)
//        //    var systemTaxCodes = _avalaraTaxManager.GetSystemTaxCodes(true)?.Select(taxCode => taxCode.taxCode).ToList();
//        //    if (!systemTaxCodes?.Any() ?? true)
//        //    {
//        //        ErrorNotification(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Delete.Error"));
//        //        return List();
//        //    }

//        //    //prepare tax categories to delete
//        //    var taxCategoriesToDelete = _taxCategoryService.GetAllTaxCategories()
//        //        .Where(taxCategory => systemTaxCodes.Contains(taxCategory.Name)).ToList();

//        //    foreach (var taxCategory in taxCategoriesToDelete)
//        //    {
//        //        //delete generic attributes
//        //        _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeDescriptionAttribute, null);
//        //        _genericAttributeService.SaveAttribute<string>(taxCategory, AvalaraTaxDefaults.TaxCodeTypeAttribute, null);

//        //        //delete tax categories
//        //        _taxCategoryService.DeleteTaxCategory(taxCategory);
//        //    }

//        //    SuccessNotification(_localizationService.GetResource("Plugins.Tax.Avalara.TaxCodes.Delete.Success"));

//        //    return List();
//        //}
//        #endregion
//    }
//}