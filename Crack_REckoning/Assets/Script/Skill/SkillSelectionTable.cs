using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionTable : DataTable
{
    Dictionary<(int, int), SkillSelectionData> table
        = new Dictionary<(int, int), SkillSelectionData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<SkillSelectionData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey((item.SkillID,item.SkillPickID)))
            {
                table.Add((item.SkillID, item.SkillPickID), item);
            }
            else
            {
                Debug.LogError("스킬 선택 중복!");
            }
        }
    }

    public SkillSelectionData Get(int id1, int id2)
    {
        if (!table.ContainsKey((id1, id2)))
        {
            return null;
        }
        return table[(id1,id2)];
    }
}
