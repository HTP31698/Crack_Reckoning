using UnityEngine;
enum MonseterWeakness
{
    Fire,
    Water,
    Lightning,
}

public class MonsterData
{
    public string MonsterID { get; set; }
    public int MonsterName { get; set; }
    public float MonsterRange { get; set; }
    public int MonsterHp { get; set; }
    public int MonsterAttack { get; set; }
    public float MonsterSpeed { get; set; }
    public float MonsterAttackSpeed { get; set; }
    public int MonsterWeakness { get; set; }
    public int MonsterStrength { get; set; }

    public string MonsterEffectID { get; set; }

    public int MonsterExp { get; set; }
    public string MonsterDec { get; set; }

    public Sprite sprite
        => Resources.Load<Sprite>($"Sprite/{MonsterName}");
    public RuntimeAnimatorController AnimatorController
        => Resources.Load<RuntimeAnimatorController>($"Animator/{MonsterName}");
}
