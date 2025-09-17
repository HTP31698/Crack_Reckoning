using System.Collections.Generic;
using UnityEngine;

public class BossTable : DataTable
{
    private readonly Dictionary<int, BossData> table = new Dictionary<int, BossData>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<BossData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.BossID))
            {
                table.Add(item.BossID, item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }
    }

    public BossData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
