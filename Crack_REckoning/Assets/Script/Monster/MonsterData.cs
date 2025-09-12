using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int MonsterID;

    public int MonsterName;
    public float MonsterRange;
    public int MonsterHp;
    public int MonsterAttack;
    public float MonsterSpeed;
    public float MonsterAttackSpeed;
    public int MonsterWeakness;
    public int MonsterStrength;

    public int MonsterEffectID;

    public int MonsterExp;
    public string MonsterAnimator;


    
}
