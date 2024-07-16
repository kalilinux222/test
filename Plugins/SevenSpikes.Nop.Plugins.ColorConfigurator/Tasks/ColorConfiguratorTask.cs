using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Tasks;
using SevenSpikes.Nop.Plugins.ColorConfigurator.Infrastructure.Constants;
using System.IO;

namespace SevenSpikes.Nop.Plugins.ColorConfigurator.Tasks
{
    public class ColorConfiguratorTask : ITask
    {
        public bool IsRunning { get; set; }

        private IWebHelper WebHelper => EngineContext.Current.Resolve<IWebHelper>();

        public void Execute()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;

            try
            {
                var imagePath = WebHelper.MapPath(Plugin.ImagesVirtualPath);

                DirectoryInfo directoryInfo = new DirectoryInfo(imagePath);

                foreach (FileInfo file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
