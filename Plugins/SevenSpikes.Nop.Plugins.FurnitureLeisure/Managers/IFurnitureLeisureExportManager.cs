using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System.Collections.Generic;
using System.IO;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers
{
	public interface IFurnitureLeisureExportManager
	{
		void ExportToCsv(MemoryStream stream, IList<ProductToProductTagsRelationModel> models);
	}
}
