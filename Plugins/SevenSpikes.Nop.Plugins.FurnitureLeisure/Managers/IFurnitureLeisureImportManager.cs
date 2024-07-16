using System.IO;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers
{
	public interface IFurnitureLeisureImportManager
	{
		void ImportCsv(Stream stream);

		void ImportCsvAll(Stream stream);
	}
}
