using CsvHelper;
using CsvHelper.TypeConversion;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public abstract class DataTable
{
    public static readonly string FormatPath = "DataTables/{0}";

    public abstract void Load(string filename);

    public static List<T> LoadCSV<T>(string csvText)
    {
        using (var reader = new StringReader(csvText))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // 타입별 ClassMap 등록
            if (typeof(T) == typeof(StageData))
                csvReader.Context.RegisterClassMap<StageDataMap>();
            else if (typeof(T) == typeof(MonsterData))
                csvReader.Context.RegisterClassMap<MonsterDataMap>();
            else
                throw new System.Exception($"ClassMap이 등록되지 않은 타입: {typeof(T)}");

            return csvReader.GetRecords<T>().ToList();
        }
    }
}