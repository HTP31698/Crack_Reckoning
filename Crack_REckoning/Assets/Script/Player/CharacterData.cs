using CsvHelper.Configuration;
using UnityEngine;

public class CharacterData
{
    public int ChID { get; set; }
    public string ChName { get; set; }
    public int BasicSkill { get; set; }
    public int ChAttack { get; set; }
    public int ChCri { get; set; }
    public float ChCriDam { get; set; }
}


public class CharacterDataMap : ClassMap<CharacterData>
{
    public CharacterDataMap()
    {
        Map(c => c.ChID).Name("ChID");
        Map(c => c.ChName).Name("ChName");
        Map(c => c.BasicSkill).Name("BasicSkill");
        Map(c => c.ChAttack).Name("ChAttack");
        Map(c => c.ChCri).Name("ChCri");
        Map(c => c.ChCriDam).Name("ChCriDam");
    }
}