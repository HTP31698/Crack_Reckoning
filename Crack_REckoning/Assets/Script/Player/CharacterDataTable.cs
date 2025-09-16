using System.Collections.Generic;
using UnityEngine;

public class CharacterDataTable : DataTable
{
    Dictionary<int, CharacterData> table = new Dictionary<int, CharacterData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<CharacterData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.ChID))
            {
                table.Add(item.ChID, item);
            }
            else
            {
                Debug.LogError("캐릭터 아이디 중복!");
            }
        }
    }

    public CharacterData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
