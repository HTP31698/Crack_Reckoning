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
}

public static class PlaySetting
{
    public static int playerID { get; set; }
    public static int SelectStage { get; set; } = 1;
    public static int PlayerBasicSkillID { get; set; }
    public static bool IsPlaying { get; set; }
    public static int gold { get; set; }
    public static void Reset()
    {
        playerID = 0;
        SelectStage = 0;
        PlayerBasicSkillID = 0;
        IsPlaying = false;
    }
}
