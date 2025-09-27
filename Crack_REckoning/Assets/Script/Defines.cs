using JetBrains.Annotations;
using UnityEngine;

public static class DataTableIds
{
    public static readonly string Monster = "MonsterTable";
    public static readonly string Stage = "StageTable";
    public static readonly string Character = "CharacterTable";
    public static readonly string Skill = "SkillTable";
    public static readonly string SkillSelection = "SkillSelectionTable";
    public static readonly string LevelUpTable = "LevelUpTable";
    public static readonly string BossTable = "BossTable";
    public static readonly string SkillEnforceTable = "SkillEnforceTable";
}

public static class PlaySetting
{
    public static int SelectStage { get; private set; }

    public static void SetSelectStage(int Stage)
    {
        SelectStage = Stage;
    }
    public static void Reset()
    { 
        SelectStage = 0;
    }

    public static bool GetStageClear(int index)
    {
        bool v;
        if (SaveLoadManager.Data.StageClear.TryGetValue(index, out v))
            return v;
        return false;
    }
}
