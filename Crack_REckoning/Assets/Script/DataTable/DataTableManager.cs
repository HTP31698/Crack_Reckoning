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
