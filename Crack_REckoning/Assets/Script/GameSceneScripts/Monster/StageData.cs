using CsvHelper.Configuration;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class StageData
{
    public int Stage {  get; set; }
    public int Wave { get; set; }
    public string StageName { get; set; }
    public int? SpawnNum { get; set; }
    public int? M1Id { get; set; }
    public int? M1Num { get; set; }
    public int? M2Id { get; set; }
    public int? M2Num { get; set; }
    public int? M3Id { get; set; }
    public int? M3Num { get; set; }
    public int? MiniBossID { get; set; }
    public int? MiniBossNum { get; set; }
    public float? WaveTime { get; set; }
    public float? StageAddMHp { get; set; }
    public float? StageAddMAtt { get; set; }

    public override string ToString()
    {
        return $"{Stage} / {StageName} / {SpawnNum} / {M1Id} / {M1Num} / {M2Id} / {M2Num} / {M3Id} / {M3Num} / {MiniBossID} / {MiniBossNum} / {WaveTime} / {StageAddMHp} / {StageAddMAtt}";
    }
}

public class StageDataMap : ClassMap<StageData>
{
    public StageDataMap()
    {
        Map(s => s.Stage).Name("Stage").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.Wave).Name("Wave").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.StageName).Name("StageName").Default(null).TypeConverterOption.NullValues("", " ");
        Map(s => s.SpawnNum).Name("SpawnNum").Default(0).TypeConverterOption.NullValues("", " ");

        Map(s => s.M1Id).Name("M1Id").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.M1Num).Name("M1Num").Default(0).TypeConverterOption.NullValues("", " ");

        Map(s => s.M2Id).Name("M2Id").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.M2Num).Name("M2Num").Default(0).TypeConverterOption.NullValues("", " ");

        Map(s => s.M3Id).Name("M3Id").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.M3Num).Name("M3Num").Default(0).TypeConverterOption.NullValues("", " ");

        Map(s => s.MiniBossID).Name("MiniBossID").Default(0).TypeConverterOption.NullValues("", " ");
        Map(s => s.MiniBossNum).Name("MiniBossNum").Default(0).TypeConverterOption.NullValues("", " ");

        Map(s => s.WaveTime).Name("WaveTime").Default(0f).TypeConverterOption.NullValues("", " ");
        Map(s => s.StageAddMHp).Name("StageAddMHp").Default(0f).TypeConverterOption.NullValues("", " ");
        Map(s => s.StageAddMAtt).Name("StageAddMAtt").Default(0f).TypeConverterOption.NullValues("", " ");
    }
}