using UnityEngine;
using CsvHelper.Configuration;

public class MonsterData
{
    public int MonsterID { get; set; }
    public string MonsterName { get; set; }
    public float MonsterRange { get; set; }
    public int MonsterHp { get; set; }
    public int MonsterAttack { get; set; }
    public float MonsterSpeed { get; set; }
    public float MonsterAttackSpeed { get; set; }
    public SkillTypeID? MonsterWeakness { get; set; }
    public SkillTypeID? MonsterStrength { get; set; }

    public int MonsterExp { get; set; }
    public int? MonsterDec { get; set; }

    public override string ToString()
    {
        return $"{MonsterID} / {MonsterName} / {MonsterRange} / {MonsterHp} / {MonsterAttack} / {MonsterSpeed} / {MonsterAttackSpeed} / {MonsterWeakness} / {MonsterStrength} / {MonsterExp} / {MonsterDec}";
    }

    public Sprite sprite
        => Resources.Load<Sprite>($"MonsterSprites/{MonsterID}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"MonsterAnimator/{MonsterID}");
}

public class MonsterDataMap : ClassMap<MonsterData>
{
    public MonsterDataMap()
    {
        Map(m => m.MonsterID).Name("MonsterID");
        Map(m => m.MonsterName).Name("MonsterName");
        Map(m => m.MonsterRange).Name("MonsterRange");
        Map(m => m.MonsterHp).Name("MonsterHp");
        Map(m => m.MonsterAttack).Name("MonsterAttack");
        Map(m => m.MonsterSpeed).Name("MonsterSpeed");
        Map(m => m.MonsterAttackSpeed).Name("MonsterAttackSpeed");
        Map(m => m.MonsterWeakness).Name("MonsterWeakness").Default(0).TypeConverterOption.NullValues("", " ");
        Map(m => m.MonsterStrength).Name("MonsterStrength").Default(0).TypeConverterOption.NullValues("", " ");
        Map(m => m.MonsterExp).Name("MonsterExp");
        Map(m => m.MonsterDec).Name("MonsterDec").Default(0).TypeConverterOption.NullValues("", " ");
    }
}
