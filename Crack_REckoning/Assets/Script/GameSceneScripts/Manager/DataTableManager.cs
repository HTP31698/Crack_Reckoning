using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public static class DataTableManager
{
    private static readonly Dictionary<string, DataTable> tables =
        new Dictionary<string, DataTable>();

    static DataTableManager()
    {
        Init();
    }

    private static void Init()
    {
        var monsterTable = new MonsterTable();
        monsterTable.Load(DataTableIds.Monster);
        tables.Add(DataTableIds.Monster, monsterTable);

        var stageTable = new StageTable();
        stageTable.Load(DataTableIds.Stage);
        tables.Add(DataTableIds.Stage, stageTable);

        var characterTable = new CharacterTable();
        characterTable.Load(DataTableIds.Character);
        tables.Add(DataTableIds.Character, characterTable);

        var skillTable = new SkillTable();
        skillTable.Load(DataTableIds.Skill);
        tables.Add(DataTableIds.Skill, skillTable);

        var skillSelectionTable = new SkillSelectionTable();
        skillSelectionTable.Load(DataTableIds.SkillSelection);
        tables.Add(DataTableIds.SkillSelection, skillSelectionTable);

        var levelUpTable = new LevelUpTable();
        levelUpTable.Load(DataTableIds.LevelUpTable);
        tables.Add(DataTableIds.LevelUpTable, levelUpTable);

        var bossTable = new BossTable();
        bossTable.Load(DataTableIds.BossTable);
        tables.Add(DataTableIds.BossTable, bossTable);

        var skillEnforceTable = new SkillEnforceTable();
        skillEnforceTable.Load(DataTableIds.SkillEnforceTable);
        tables.Add(DataTableIds.SkillEnforceTable, skillEnforceTable);

        var petTable = new PetTable();
        petTable.Load(DataTableIds.PetTable);
        tables.Add(DataTableIds.PetTable, petTable);

        var petEnforceTable = new PetEnforceTable();
        petEnforceTable.Load(DataTableIds.PetEnforceTable);
        tables.Add(DataTableIds.PetEnforceTable, petEnforceTable);

        var characterEnforceTable = new CharacterEnforceTable();
        characterEnforceTable.Load(DataTableIds.CharacterEnforceTable);
        tables.Add(DataTableIds.CharacterEnforceTable, characterEnforceTable);
    }

    public static MonsterTable MonsterTable
    {
        get
        {
            return Get<MonsterTable>(DataTableIds.Monster);
        }
    }

    public static StageTable StageTable
    {
        get
        {
            return Get<StageTable>(DataTableIds.Stage);
        }
    }

    public static CharacterTable CharacterTable
    {
        get
        {
            return Get<CharacterTable>(DataTableIds.Character);
        }
    }

    public static SkillTable SkillTable
    {
        get
        {
            return Get<SkillTable>(DataTableIds.Skill);
        }
    }

    public static SkillSelectionTable SkillSelectionTable
    {
        get
        {
            return Get<SkillSelectionTable>(DataTableIds.SkillSelection);
        }
    }

    public static LevelUpTable LevelUpTable
    {
        get
        {
            return Get<LevelUpTable>(DataTableIds.LevelUpTable);
        }
    }

    public static BossTable BossTable
    {
        get
        {
            return Get<BossTable>(DataTableIds.BossTable);
        }
    }

    public static SkillEnforceTable SkillEnforceTable
    {
        get
        {
            return Get<SkillEnforceTable>(DataTableIds.SkillEnforceTable);
        }
    }

    public static PetTable PetTable
    {
        get
        {
            return Get<PetTable>(DataTableIds.PetTable);
        }
    }

    public static PetEnforceTable PetEnforceTable
    {
        get
        {
            return Get<PetEnforceTable>(DataTableIds.PetEnforceTable);
        }
    }

    public static CharacterEnforceTable CharacterEnforceTable
    {
        get
        {
            return Get<CharacterEnforceTable>(DataTableIds.CharacterEnforceTable);
        }
    }

    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.LogError("테이블 없음");
            return null;
        }
        return tables[id] as T;
    }
}
