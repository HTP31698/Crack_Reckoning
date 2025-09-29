using System.Collections.Generic;
using UnityEngine;

public class PetTable : DataTable
{
    private readonly Dictionary<int, PetData> table = new Dictionary<int, PetData>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<PetData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.PetID))
            {
                table.Add(item.PetID, item);
            }
            else
            {
                Debug.LogError("�� ���̵� �ߺ�!");
            }
        }
    }

    public PetData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
