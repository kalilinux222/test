using CsvHelper;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Managers
{
	public class FurnitureLeisureExportManager : IFurnitureLeisureExportManager
	{
		public void ExportToCsv(MemoryStream stream, IList<ProductToProductTagsRelationModel> models)
		{
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(models);
            }
		}
	}
}
