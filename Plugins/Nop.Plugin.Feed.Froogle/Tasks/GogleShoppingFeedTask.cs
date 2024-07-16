using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Logging;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Feed.Froogle.Tasks
{
    public class GogleShoppingFeedTask : ITask
    {
        private ILogger _logger;
        private IPluginFinder _pluginFinder;
        private IStoreService _storeService;
        private FroogleSettings _froogleSettings;

        private bool IsRunning { get; set; }

        private ILogger Logger
        {
            get
            {
                if(_logger == null)
                {
                    _logger = EngineContext.Current.Resolve<ILogger>();
                }

                return _logger;
            }
        }

        private IPluginFinder PluginFinder
        {
            get
            {
                if(_pluginFinder == null)
                {
                    _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
                }

                return _pluginFinder;
            }
        }

        private IStoreService StoreService
        {
            get
            {
                if(_storeService == null)
                {
                    _storeService = EngineContext.Current.Resolve<IStoreService>();
                }

                return _storeService;
            }
        }

        private FroogleSettings FroogleSettings
        {
            get
            {
                if(_froogleSettings == null)
                {
                    _froogleSettings = EngineContext.Current.Resolve<FroogleSettings>();
                }

                return _froogleSettings;
            }
        }

        public void Execute()
        {
            if(IsRunning)
            {
                return;
            }

            IsRunning = true;

            try
            {
                var pluginDescriptor = PluginFinder.GetPluginDescriptorBySystemName("PromotionFeed.Froogle");
                if (pluginDescriptor == null)
                    throw new Exception("Cannot load the plugin");

                //plugin
                var plugin = pluginDescriptor.Instance() as FroogleService;
                if (plugin == null)
                    throw new Exception("Cannot load the plugin");

                var stores = new List<Store>();
                var storeById = StoreService.GetStoreById(FroogleSettings.StoreId);
                if (storeById != null)
                    stores.Add(storeById);
                else
                    stores.AddRange(StoreService.GetAllStores());

                foreach (var store in stores)
                    plugin.GenerateStaticFile(store);
            }
            catch(Exception e)
            {
                var errorMessage = "An error occured while running the GoogleShoppingFeedTask";

                Logger.InsertLog(LogLevel.Error, errorMessage, e.Message);
            }
            finally
            {
                IsRunning = false;
            }
        }

    }
}
