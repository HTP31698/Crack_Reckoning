using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SaveData
{
    public int Version { get; protected set; }

    public abstract SaveData VersionUp();
}

[Serializable]
public class SaveDataV1 : SaveData
{
    public int PlayerID { get; set; } = 0;
    public int Gold { get; set; } = 0;
    public List<int> OwnedSkillIds = new List<int>();
    public List<int> EquipmentSkillIds = new List<int>();
    public Dictionary<int, bool> StageClear = new Dictionary<int, bool>();
    public int CurrentCrack { get; set; } = 0;

    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        throw new NotImplementedException();
    }
}

