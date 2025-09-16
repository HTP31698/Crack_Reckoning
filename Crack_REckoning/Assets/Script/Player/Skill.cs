using UnityEngine;

public class Skill : MonoBehaviour
{
    

    public int skillID;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private SkillTable skillTable;
    private SkillData skillData;

    private int id;
    public string skillName;
    public SkillSortationID skillSortationID;
    public SkillTypeID skillTypeID;
    public float skillRange;
    public int skillDamage;
    public float skillCoolTime;
    public int projectilesNum;
    public int attackNum;
    public int penetratingPower;
    public float SkillDamageRange;
    public int effectID;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        
    }

    public void Init(int id)
    {
        this.id = id;
        
    }
    public void SetTarget(Vector3 targetpos)
    {
        
    }

}