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
            // Ÿ�Ժ� ClassMap ���
            if (typeof(T) == typeof(StageData))
                csvReader.Context.RegisterClassMap<StageDataMap>();
            else if (typeof(T) == typeof(MonsterData))
                csvReader.Context.RegisterClassMap<MonsterDataMap>();
            else
                throw new System.Exception($"ClassMap�� ��ϵ��� ���� Ÿ��: {typeof(T)}");

            return csvReader.GetRecords<T>().ToList();
        }
    }
}