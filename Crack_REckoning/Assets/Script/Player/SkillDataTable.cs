using System.Collections.Generic;
using UnityEngine;

public class SkillDataTable : DataTable
{
    Dictionary<int, SkillData> table = new Dictionary<int, SkillData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<SkillData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.SkillID))
            {
                table.Add(item.SkillID, item);
            }
            else
            {
                Debug.LogError("스킬 아이디 중복");
            }
        }
    }

    public SkillData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}
