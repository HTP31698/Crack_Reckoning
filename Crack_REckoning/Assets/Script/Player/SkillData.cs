using CsvHelper.Configuration;
using UnityEngine;

enum SkillSortationID
{
    BasicSkill,
    StageSkill,
    PetSkill,
}

enum SkillTypeID
{
    fire,
    water,
    ice,
    lightning,
    physical,
}

public class SkillData
{
    public int SkillID { get; set; }
    public string SkillName { get; set; }
    public int SkillSortation { get; set; }
    public int SkillType { get; set; }
    public float SkillRange { get; set; }
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }
    public int? EffectID { get; set; }
}

public class SkillDataMap : ClassMap<SkillData>
{
    public SkillDataMap()
    {
        Map(s => s.SkillID).Name("SkillID");
        Map(s => s.SkillName).Name("SkillName");
        Map(s => s.SkillSortation).Name("SkillSortation");
        Map(s => s.SkillType).Name("SkillType");
        Map(s => s.SkillRange).Name("SkillRange");
        Map(s => s.SkillDamage).Name("SkillDamage");
        Map(s => s.SkillCoolTime).Name("SkillCoolTime");
        Map(s => s.ProjectilesNum).Name("ProjectilesNum");
        Map(s => s.AttackNum).Name("AttackNum");
        Map(s => s.PenetratingPower).Name("PenetratingPower");
        Map(s => s.SkillDamageRange).Name("SkillDamageRange");
        Map(s => s.EffectID).Name("EffectID").Default(0).TypeConverterOption.NullValues("", " ");
    }
}
