using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StageDataTable : DataTable
{
    public List<StageData> stagelist = new List<StageData>();

    public override void Load(string filename)
    {
        stagelist.Clear();
        var path = string.Format(FormatPath, filename);
        var textAsset = Resources.Load<TextAsset>(path);

        if (textAsset == null)
        {
            Debug.LogError($"CSV ���� �� ã��: {path}");
            return;
        }

        stagelist = LoadCSV<StageData>(textAsset.text);
    }

    public StageData GetAtIndex(int index)
    {
        if (index < 0 || index >= stagelist.Count)
        {
            Debug.LogWarning($"�ε��� ���� �ʰ�: {index}");
            return null;
        }
        return stagelist[index];
    }
}
