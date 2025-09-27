using System.Collections.Generic;
using UnityEngine;

public class SkillEnforceTable :DataTable
{
    Dictionary<int, SkillEnforceData> table = new Dictionary<int, SkillEnforceData>();
    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<SkillEnforceData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.EnhanceID))
            {
                table.Add(item.EnhanceID, item);
            }
            else
            {
                Debug.LogError($"{item.EnhanceID}��ų ���̵� �ߺ�");
            }
        }
    }

    public SkillEnforceData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }

}
