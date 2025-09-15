using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionDataTable : DataTable
{
    Dictionary<(int, int), SkillSelectionData> table
        = new Dictionary<(int, int), SkillSelectionData>();
    public override void Load(string filename)
    {
        throw new System.NotImplementedException();
    }
}
