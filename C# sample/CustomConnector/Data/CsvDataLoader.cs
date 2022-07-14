using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

using CustomConnector.Models;

using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Graph.Connectors.Contracts.Grpc;

namespace CustomConnector.Data
{
    public static class CsvDataLoader
    {
        public static void ReadRecordFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<AppliancePartMap>();
                csv.Read();
            }
        }

        public static IEnumerable<CrawlItem> GetCrawlItemsFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<AppliancePartMap>();

                // The GetRecords<T> method will return an IEnumerable<T> that will yield records. What this means is that only a single record is returned at a time as you iterate the records.
                foreach (var record in csv.GetRecords<AppliancePart>())
                {
                    yield return record.ToCrawlItem();
                }
            }
        }
    }

    public class ApplianceListConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var appliances = text.Split(';');
            return new List<string>(appliances);
        }
    }

    public class AppliancePartMap : ClassMap<AppliancePart>
    {
        public AppliancePartMap()
        {
            Map(m => m.PartNumber);
            Map(m => m.Name);
            Map(m => m.Description);
            Map(m => m.Price);
            Map(m => m.Inventory);
            Map(m => m.Appliances).TypeConverter<ApplianceListConverter>();
        }
    }
}
