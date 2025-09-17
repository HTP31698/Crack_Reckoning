using CsvHelper.Configuration;
using UnityEngine;

public class LevelUpData
{
    public int LvName {get; set;}
    public int LvExp {get; set;}
}

public class LevelUpDataMap : ClassMap<LevelUpData>
{
    public LevelUpDataMap()
    {
        Map(l => l.LvName).Name("LvName");
        Map(l => l.LvExp).Name("LvExp");
    }
}