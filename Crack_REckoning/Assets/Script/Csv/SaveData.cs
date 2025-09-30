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

    public List<bool> GetSkills = new List<bool>();
    public List<bool> GetPets = new List<bool>();

    public List<int> OwnedPetIds = new List<int>();
    public Dictionary<int, int> EquipmentPetIds = new Dictionary<int, int>();
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

