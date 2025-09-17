using UnityEngine;

public static class DataTableIds
{
    public static readonly string Monster = "MonsterTable";
    public static readonly string Stage = "StageTable";
    public static readonly string Character = "CharacterTable";
    public static readonly string Skill = "SkillTable";
    public static readonly string SkillSelection = "SkillSelectionTable";
    public static readonly string LevelUpTable = "LevelUpTable";
}

public static class PlaySetting
{
    public static int playerID;
    public static int SelectStage;
    public static int PlayerBasicSkillID;
    public static bool IsPlaying;
    public static void Reset()
    {
        playerID = 0;
        SelectStage = 0;
        PlayerBasicSkillID = 0;
        IsPlaying = false;
    }
}
