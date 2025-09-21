using UnityEngine;
using CsvHelper.Configuration;

public class BossData
{
    public int BossID { get; set; }
    public string BossName { get; set; }
    public float BossRange { get; set; }
    public int BossHp { get; set; }
    public int BossAttack { get; set; }
    public float BossSpeed { get; set; }
    public float BossAttackSpeed { get; set; }
    public SkillTypeID? BossWeakness { get; set; }
    public SkillTypeID? BossStrength { get; set; }

    public int? BossEffectID { get; set; }

    public bool IsMiniBoss { get; set; }

    public override string ToString()
    {
        return $"{BossID} / {BossName} / {BossRange} / {BossHp} / {BossAttack} / {BossSpeed} / {BossAttackSpeed} / {BossWeakness} / {BossStrength} / {BossEffectID} / {IsMiniBoss}";
    }

    public Sprite sprite
        => Resources.Load<Sprite>($"Sprites/{BossID}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"Animator/{BossID}");
}

public class BossDataMap : ClassMap<BossData>
{
    public BossDataMap()
    {
        Map(m => m.BossID).Name("BossID");
        Map(m => m.BossName).Name("BossName");
        Map(m => m.BossRange).Name("BossRange");
        Map(m => m.BossHp).Name("BossHp");
        Map(m => m.BossAttack).Name("BossAttack");
        Map(m => m.BossSpeed).Name("BossSpeed");
        Map(m => m.BossAttackSpeed).Name("BossAttackSpeed");
        Map(m => m.BossWeakness).Name("BossWeakness").Default(0).TypeConverterOption.NullValues("", " "); ;
        Map(m => m.BossStrength).Name("BossStrength").Default(0).TypeConverterOption.NullValues("", " "); ;
        Map(m => m.BossEffectID).Name("BossEffectID").Default(0).TypeConverterOption.NullValues("", " "); ;
        Map(m => m.IsMiniBoss).Name("IsMiniBoss");
    }
}
