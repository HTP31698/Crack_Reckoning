using CsvHelper.Configuration;
using UnityEngine;

public enum SkillTypeID
{
    Fire = 1,
    Water = 2,
    Ice = 3,
    Lightning = 4,
    Dark = 5,
}

public enum AttackTypeID
{
    Projectile,
    Area,
    Explosion,
    Laser,
    Haeil,
    IceSheet,
    BlackHole,
    Mine,
    ElectricSphere,
}

public class SkillData
{
    public int SkillID { get; set; }
    public string SkillName { get; set; }
    public int SkillType { get; set; }
    public float SkillRange { get; set; }
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }
    public AttackTypeID AttackType { get; set; }
    public string SkillDescription { get; set; }

    public float? ExplosionRange { get; set; }
    public float? ExplosionDamage { get; set; }
    public float? FreezeTime { get; set; }
    public float? StunTime { get; set; }
    public float? Duration { get; set; }
    public float? PerSecond { get; set; }
    public float? KonckBack { get; set; }
    public float? Strain {  get; set; }

    public float AuthorRadius { get; set; }

    public int SoundAttackID { get; set; }
    public int SoundHitID { get; set; }


    public override string ToString()
    {
        return $"{SkillID} / {SkillName} / {SkillType} / {SkillRange} / {SkillDamage} / {SkillCoolTime} / {ProjectilesNum} / {AttackNum} / {PenetratingPower} / {SkillDamageRange} / {AttackType} / {SkillDescription} / {ExplosionRange} / {ExplosionDamage} / {FreezeTime} / {StunTime} / {Duration} / {PerSecond} / {KonckBack} / {Strain} / {AuthorRadius} / {SoundAttackID} / {SoundHitID}";
    }

    public Sprite sprite
        => Resources.Load<Sprite>($"SkillSprite/{SkillID}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"SkillAnimator/{SkillID}");

    public Sprite TypeSprite
        => Resources.Load<Sprite>($"SkillTypeSprite/{SkillType}");

    public GameObject SkillParticle
        => Resources.Load<GameObject>($"SkillParticle/{SkillID}");

    public Material Material
        => Resources.Load<Material>($"SkillMarterial/{SkillID}");

    public AudioClip attackaudioClip
        => Resources.Load<AudioClip>($"SoundAttack/{SoundAttackID}");
    public AudioClip hitaudioClip
        => Resources.Load<AudioClip>($"SoundHit/{SoundHitID}");

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
        AttackType = AttackType,
        ExplosionRange = ExplosionRange,
        ExplosionDamage = ExplosionDamage,
        FreezeTime = FreezeTime,
        StunTime = StunTime,
        Duration = Duration,
        PerSecond = PerSecond,
        KonckBack = KonckBack,
        Strain = Strain,
        AuthorRadius= AuthorRadius,
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
        Map(s => s.SkillDescription).Name("SkillDescription");
        Map(s => s.ExplosionRange).Name("ExplosionRange").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.ExplosionDamage).Name("ExplosionDamage").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.FreezeTime).Name("FreezeTime").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.StunTime).Name("StunTime").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.Duration).Name("Duration").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.PerSecond).Name("PerSecond").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.KonckBack).Name("KonckBack").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.Strain).Name("Strain").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.AuthorRadius).Name("AuthorRadius").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.SoundAttackID).Name("SoundAttackID");
        Map(s => s.SoundHitID).Name("SoundHitID");
    }
}
