using UnityEngine;
enum MonseterWeakness
{
    Fire,
    Water,
    Lightning,
}
enum MonseterStrength
{
    Fire,
    Water, 
    Lightning,
};

public class MonsterData
{
    public int MonsterID { get; set; }
    public string MonsterName { get; set; }
    public float MonsterRange { get; set; }
    public int MonsterHp { get; set; }
    public int MonsterDamage { get; set; }
    public float MonsterSpeed { get; set; }
    public float MonsterAttackSpeed { get; set; }
    public int MonsterWeakness { get; set; }
    public int MonsterStrength { get; set; }

    public int MonsterEffectID { get; set; }

    public int MonsterExp { get; set; }
    public int MonsterDec { get; set; }

    public override string ToString()
    {
        return $"{MonsterID} / {MonsterName} / {MonsterRange} / {MonsterHp} / {MonsterDamage} / {MonsterSpeed} / {MonsterAttackSpeed} / {MonsterWeakness} / {MonsterStrength} / {MonsterEffectID} / {MonsterExp} / {MonsterDec}";
    }

    public Sprite sprite
        => Resources.Load<Sprite>($"Sprite/{MonsterName}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"Animator/{MonsterName}");

}
