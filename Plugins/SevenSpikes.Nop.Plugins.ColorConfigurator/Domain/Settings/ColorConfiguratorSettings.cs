using Nop.Core.Configuration;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Domain.Settings
{
    public class ColorConfiguratorSettings : ISettings
    {
        public int ScheduledTaskId { get; set; }

        public int ScheduledTaskIntervalInSeconds { get; set; }
    }
}