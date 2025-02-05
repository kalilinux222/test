﻿using Nop.Admin.Controllers;
using Nop.Plugin.Tax.Avalara.Models.Log;
using Nop.Plugin.Tax.Avalara.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Web.Framework.Kendoui;
using HtmlHelper = Nop.Core.Html.HtmlHelper;

namespace Nop.Plugin.Tax.Avalara.Controllers
{
    public partial class TaxTransactionLogController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ITaxTransactionLogService _taxTransactionLogService;

        #endregion

        #region Ctor

        public TaxTransactionLogController(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ITaxTransactionLogService taxTransactionLogService)
        {
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._taxTransactionLogService = taxTransactionLogService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual ActionResult LogList(DataSourceRequest command, TaxTransactionLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            //prepare filter parameters
            var createdFromValue = searchModel.CreatedFrom.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone) : null;
            var createdToValue = searchModel.CreatedTo.HasValue
                ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1) : null;

            //get tax transaction log
            var taxtransactionLog = _taxTransactionLogService.GetTaxTransactionLog(createdFromUtc: createdFromValue,
                createdToUtc: createdToValue, pageIndex: command.Page - 1, pageSize: command.PageSize);

            //prepare grid model
            var gridModel = new DataSourceResult();

            gridModel.Data = taxtransactionLog.Select(logItem => new TaxTransactionLogModel
            {
                Id = logItem.Id,
                StatusCode = logItem.StatusCode,
                Url = logItem.Url,
                CustomerId = logItem.CustomerId,
                CreatedDate = _dateTimeHelper.ConvertToUserTime(logItem.CreatedDateUtc, DateTimeKind.Utc)
            });

            gridModel.Total = taxtransactionLog.TotalCount;

            return Json(gridModel);
        }

        public virtual ActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            _taxTransactionLogService.ClearTaxTransactionLog();

            return Json(new { result = true });
        }

        public virtual ActionResult View(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            //try to get log item with the passed identifier
            var logItem = _taxTransactionLogService.GetTaxTransactionLogById(id);
            if (logItem == null)
                return RedirectToAction("Configure", "AvalaraTax");

            var model = new TaxTransactionLogModel
            {
                Id = logItem.Id,
                StatusCode = logItem.StatusCode,
                Url = logItem.Url,
                RequestMessage = HtmlHelper.FormatText(logItem.RequestMessage, false, true, false, false, false, false),
                ResponseMessage = HtmlHelper.FormatText(logItem.ResponseMessage, false, true, false, false, false, false),
                CustomerId = logItem.CustomerId,
                CustomerEmail = _customerService.GetCustomerById(logItem.CustomerId)?.Email,
                CreatedDate = _dateTimeHelper.ConvertToUserTime(logItem.CreatedDateUtc, DateTimeKind.Utc)
            };

            return View("~/Plugins/Tax.Avalara/Views/Log/View.cshtml", model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            //try to get log item with the passed identifier
            var logItem = _taxTransactionLogService.GetTaxTransactionLogById(id);
            if (logItem != null)
            {
                _taxTransactionLogService.DeleteTaxTransactionLog(logItem);
                SuccessNotification(_localizationService.GetResource("Plugins.Tax.Avalara.Log.Deleted"));
            }

            return RedirectToAction("Configure", "AvalaraTax");
        }

        #endregion
    }
}