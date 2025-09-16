using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterTable : DataTable
{
    private readonly Dictionary<int, MonsterData> table = new Dictionary<int, MonsterData>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<MonsterData>(textAsset.text);

        foreach(var item in list)
        {
            if(!table.ContainsKey(item.MonsterID))
            {
                table.Add(item.MonsterID, item);
            }
            else
            {
                Debug.LogError("���� ���̵� �ߺ�!");
            }
        }
    }

    public MonsterData Get(int id)
    {
        if(!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
