using CsvHelper.Configuration;
using UnityEngine;

public enum SkillTypeID
{
    Fire,
    Water,
    Ice,
    Lightning,
    Physical,
}

public enum AttackTypeID
{
    Projectile,
    Area,
}

public class SkillData
{
    public int SkillID { get; set; }
    public string SkillName { get; set; }
    public SkillTypeID SkillType { get; set; }
    public float SkillRange { get; set; }
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }
    public AttackTypeID AttackType { get; set; }

    public override string ToString()
    {
        return $"{SkillID} / {SkillName} / {SkillType} / {SkillRange} / {SkillDamage} / {SkillCoolTime} / {ProjectilesNum} / {AttackNum} / {PenetratingPower} / {SkillDamageRange} / {AttackType}";
    }

    public Sprite sprite
        => Resources.Load<Sprite>($"Sprite/{SkillID}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"Animator/{SkillID}");

    public SkillData Clone() => new SkillData
    {
        SkillID = SkillID,
        SkillName = SkillName,
        SkillType = SkillType,
        SkillRange = SkillRange,
        SkillDamage = SkillDamage,
        SkillCoolTime = SkillCoolTime,
        ProjectilesNum = ProjectilesNum,
        AttackNum = AttackNum,
        PenetratingPower = PenetratingPower,
        SkillDamageRange = SkillDamageRange,
        AttackType = AttackType
    };
}

public class SkillDataMap : ClassMap<SkillData>
{
    public SkillDataMap()
    {
        Map(s => s.SkillID).Name("SkillID");
        Map(s => s.SkillName).Name("SkillName");
        Map(s => s.SkillType).Name("SkillType");
        Map(s => s.SkillRange).Name("SkillRange");
        Map(s => s.SkillDamage).Name("SkillDamage");
        Map(s => s.SkillCoolTime).Name("SkillCoolTime");
        Map(s => s.ProjectilesNum).Name("ProjectilesNum");
        Map(s => s.AttackNum).Name("AttackNum");
        Map(s => s.PenetratingPower).Name("PenetratingPower");
        Map(s => s.SkillDamageRange).Name("SkillDamageRange");
        Map(s => s.AttackType).Name("AttackType");
    }
}
