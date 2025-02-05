﻿using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;

namespace Nop.Plugin.Tax.Avalara.Models.Log
{
    /// <summary>
    /// Represents a tax transaction log model
    /// </summary>
    public partial class TaxTransactionLogModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.StatusCode")]
        public int StatusCode { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.RequestMessage")]
        public string RequestMessage { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.ResponseMessage")]
        public string ResponseMessage { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.Customer")]
        public int? CustomerId { get; set; }
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Avalara.Log.CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }
}