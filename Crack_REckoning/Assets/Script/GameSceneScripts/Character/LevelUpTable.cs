using System.Collections.Generic;
using UnityEngine;

public class LevelUpTable : DataTable
{
    Dictionary<int, LevelUpData> table = new Dictionary<int, LevelUpData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<LevelUpData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.LvName))
            {
                table.Add(item.LvName, item);
            }
            else
            {
                Debug.LogError("레벨 중복!");
            }
        }
    }

    public LevelUpData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
