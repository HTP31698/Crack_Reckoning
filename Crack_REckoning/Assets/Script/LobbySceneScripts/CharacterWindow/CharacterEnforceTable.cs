using System.Collections.Generic;
using UnityEngine;

public class CharacterEnforceTable : DataTable
{
    Dictionary<int, CharacterEnforceData> table = new Dictionary<int, CharacterEnforceData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<CharacterEnforceData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.EnhanceID))
            {
                table.Add(item.EnhanceID, item);
            }
            else
            {
                Debug.LogError("캐릭터 아이디 중복");
            }
        }
    }

    public CharacterEnforceData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
