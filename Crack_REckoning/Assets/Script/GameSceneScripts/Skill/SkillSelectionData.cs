using CsvHelper.Configuration;
using UnityEngine;

public class SkillSelectionData
{
    public int SkillID { get; set; }
    public int SkillPickID { get; set; }
    public string SkillPickName { get; set; }
    public float SkillDamageNumChange { get; set; }
    public float? IncreasingSkillDamageRange { get; set; }
    public float? ReduceSkillCT { get; set; }
    public int? IncreasedProjectile { get; set; }
    public int? IncreaseNumAttack { get; set; }
    public int? PeneTratingPower { get; set; }
}

public class SkillSelectionDataMap : ClassMap<SkillSelectionData>
{
    public SkillSelectionDataMap()
    {
        Map(s => s.SkillID).Name("SkillID");
        Map(s => s.SkillPickID).Name("SkillPickID");
        Map(s => s.SkillPickName).Name("SkillPickName");
        Map(s => s.SkillDamageNumChange).Name("SkillDamageNumChange");
        Map(s => s.IncreasingSkillDamageRange).Name("IncreasingSkillDamageRange").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.ReduceSkillCT).Name("ReduceSkillCT").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.IncreasedProjectile).Name("IncreasedProjectile").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.IncreaseNumAttack).Name("IncreaseNumAttack").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.PeneTratingPower).Name("PeneTratingPower").Default(0).TypeConverterOption.NullValues("", " ");
    }
}
