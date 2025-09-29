using System.Collections.Generic;
using UnityEngine;

public class PetEnforceTable : DataTable
{
    Dictionary<int, PetEnforceData> table = new Dictionary<int, PetEnforceData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<PetEnforceData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.EnhanceID))
            {
                table.Add(item.EnhanceID, item);
            }
            else
            {
                Debug.LogError("펫 아이디 중복");
            }
        }
    }

    public PetEnforceData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
