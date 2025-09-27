using CsvHelper.Configuration;
using UnityEngine;

public class SkillEnforceData
{
    public int EnhanceID { get; set; }
    public int EnhanceNum { get; set; }
    public int GoldNum { get; set; }
    public int SuccessPercent { get; set; }
    public int ResultRewards { get; set; }

    public override string ToString()
    {
        return $"{EnhanceID} / {EnhanceNum} / {GoldNum} / {SuccessPercent} / {ResultRewards}";
    }
}


public class SkillEnforceDataMap : ClassMap<SkillEnforceData>
{
    public SkillEnforceDataMap()
    {
        Map(s => s.EnhanceID).Name("EnhanceID");
        Map(s => s.EnhanceNum).Name("EnhanceNum");
        Map(s => s.GoldNum).Name("GoldNum");
        Map(s => s.SuccessPercent).Name("SuccessPercent");
        Map(s => s.ResultRewards).Name("ResultRewards");
    }
}
