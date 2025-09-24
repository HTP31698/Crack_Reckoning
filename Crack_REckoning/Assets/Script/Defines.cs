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
    public static int PlayerID { get; private set; }
    public static int SelectStage { get; private set; }
    public static int CurrnetSelectSkillID { get; private set; }
    public static int Gold { get; private set; }

    public static void SetPlayerId(int playerID)
    {
        PlayerID = playerID;
    }
    public static void SetSelectStage(int Stage)
    {
        SelectStage = Stage;
    }
    public static void SetCurrnetSelectSkillID(int currnetSelectSkillID)
    {
        CurrnetSelectSkillID = currnetSelectSkillID;
    }
    public static void SetClearGold(int amount)
    {
        Gold += amount;
    }

    public static void Reset()
    {
        PlayerID = 0;
        SelectStage = 0;
        CurrnetSelectSkillID = 0;
        Gold = 0;
    }
}
