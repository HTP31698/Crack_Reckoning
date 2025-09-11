using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public static class DataTableManger
{
    private static readonly Dictionary<string, DataTable> tables =
        new Dictionary<string, DataTable>();

    static DataTableManger()
    {
        Init();
    }

    private static void Init()
    {
#if UNITY_EDITOR
        //foreach (var id in DataTableIds.StringTableIds)
        //{
        //    var table = new StringTable();
        //    table.Load(id);
        //    tables.Add(id, table);
        //}
#else
        var stringTable = new StringTable();
        stringTable.Load(DataTableIds.String);
        tables.Add(DataTableIds.String, stringTable);
#endif

        //var itemTable = new ItemTable();
        //itemTable.Load(DataTableIds.Item);
        //tables.Add(DataTableIds.Item, itemTable);

        //var diffyTable = new DifficultyTable();
        //diffyTable.Load(DataTableIds.Difficulty);
        //dtables.Add(DataTableIds.Difficulty, diffyTable);
    }

    //public static StringTable StringTable
    //{
    //    get
    //    {
    //        return Get<StringTable>(DataTableIds.String);
    //    }
    //}

    //public static ItemTable ItemTable
    //{
    //    get
    //    {
    //        return Get<ItemTable>(DataTableIds.Item);
    //    }
    //}

    //public static DifficultyTable DifficultyTable
    //{
    //    get
    //    {
    //        return Get<DifficultyTable>(DataTableIds.Difficulty);
    //    }
    //}

    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.LogError("테이블 없음");
            return null;
        }
        return tables[id] as T;
    }
}
