using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.JsonLD.Domain;
using SevenSpikes.Nop.Plugins.JsonLD.Helpers;
using SevenSpikes.Nop.Plugins.JsonLD.Infrastructure.Constants;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.JsonLD.ActionFilters
{
    public class JsonLdCategoryFilterAttribute : ActionFilterAttribute
    {
        private JsonLdSettings _jsonLdSettings;
        private WidgetSettings _widgetSettings;
        private IStoreContext _storeContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private IProductCustomPropertiesHelper _productCustomPropertiesHelper;

        private JsonLdSettings JsonLdSettings
        {
            get
            {
                if(_jsonLdSettings == null)
                {
                    _jsonLdSettings = EngineContext.Current.Resolve<JsonLdSettings>();
                }

                return _jsonLdSettings;
            }
        }

        private WidgetSettings WidgetSettings
        {
            get
            {
                if(_widgetSettings == null)
                {
                    _widgetSettings = EngineContext.Current.Resolve<WidgetSettings>();
                }

                return _widgetSettings;
            }
        }

        private IStoreContext StoreContext
        {
            get
            {
                if(_storeContext == null)
                {
                    _storeContext = EngineContext.Current.Resolve<IStoreContext>();
                }

                return _storeContext;
            }
        }

        private IWorkContext WorkContext
        {
            get
            {
                if(_workContext == null)
                {
                    _workContext = EngineContext.Current.Resolve<IWorkContext>();
                }

                return _workContext;
            }
        }

        private ICacheManager CacheManager
        {
            get
            {
                if(_cacheManager == null)
                {
                    _cacheManager = EngineContext.Current.Resolve<ICacheManager>();
                }

                return _cacheManager;
            }
        }

        private IProductCustomPropertiesHelper ProductCustomPropertiesHelper
        {
            get
            {
                if (_productCustomPropertiesHelper == null)
                {
                    _productCustomPropertiesHelper = EngineContext.Current.Resolve<IProductCustomPropertiesHelper>();
                }

                return _productCustomPropertiesHelper;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if(JsonLdSettings.Enable && WidgetSettings.ActiveWidgetSystemNames.Contains(Plugin.SystemName))
            {
                var model = filterContext.Controller.ViewData.Model;

                if (filterContext.RouteData.Values.TryGetValue("categoryid", out var categoryId))
                {
                    if (model != null && model is CategoryModel)
                    {
                        var categoryModel = model as CategoryModel;
                        
                        ProductCustomPropertiesHelper.AddCustomProperties(categoryModel.Products);

                        var categoryKey = string.Format(Plugin.JSONLD_CATEGORY_CACHE_KEY,
                            categoryId, WorkContext.CurrentCustomer.Id,
                            WorkContext.WorkingLanguage.Id, StoreContext.CurrentStore.Id );

                        CacheManager.Set(categoryKey, categoryModel, 0);
                    }
                }
            }
        }
    }
}
