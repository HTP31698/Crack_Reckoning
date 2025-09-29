using CsvHelper.Configuration;
using UnityEngine;

public class PetData
{
    public int PetID { get; set; }
    public string PetName { get; set; }
    public float? GoldUp { get; set; }
    public float? WaveTime { get; set; }
    public int? WallHpUp { get; set; }
    public int? AttBuff { get; set; }

    public override string ToString()
    {
        return $"{PetID} / {PetName} / {GoldUp} / {WaveTime} / {WallHpUp} / {AttBuff}";
    }

    public Sprite sprite
       => Resources.Load<Sprite>($"PetSprite/{PetID}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"PetAnimator/{PetID}");
}

public class PetDataMap : ClassMap<PetData>
{
    public PetDataMap()
    {
        Map(p => p.PetID).Name("PetID");
        Map(p => p.PetName).Name("PetName");
        Map(p => p.GoldUp).Name("GoldUp").Default(0).TypeConverterOption.NullValues("", " ");
        Map(p => p.WaveTime).Name("WaveTime").Default(0).TypeConverterOption.NullValues("", " ");
        Map(p => p.WallHpUp).Name("WallHpUp").Default(0).TypeConverterOption.NullValues("", " ");
        Map(p => p.AttBuff).Name("AttBuff").Default(0).TypeConverterOption.NullValues("", " ");
    }
}