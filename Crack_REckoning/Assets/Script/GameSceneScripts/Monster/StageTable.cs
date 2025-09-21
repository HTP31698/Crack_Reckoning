using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StageTable : DataTable
{
    public Dictionary<(int ,int), StageData> table = new Dictionary<(int ,int), StageData>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<StageData>(textAsset.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey((item.Stage, item.Wave)))
            {
                table.Add((item.Stage,item.Wave), item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }
    }

    public StageData Get(int id1, int id2)
    {
        if (!table.ContainsKey((id1,id2)))
        {
            return null;
        }
        return table[(id1,id2)];
    }
}
