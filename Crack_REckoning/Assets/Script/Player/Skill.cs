using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.U2D;

public class Skill : MonoBehaviour
{
    private static readonly string SkillTable = "SkillTable";

    public int skillID;
    private Vector3 targetpos;
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
    private Sprite sprite;
    private RuntimeAnimatorController controller;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(int id)
    {
        this.id = id;
        skillTable = DataTableManager.Get<SkillTable>(SkillTable);
        InitSkillData();
    }
    private void InitSkillData()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (skillTable != null)
        {
            skillData = skillTable.Get(id);
            if (skillData != null)
            {
                 id = skillData.SkillID;
                 skillName = skillData.SkillName;
                 skillSortationID = skillData.SkillSortation;
                 skillTypeID = skillData.SkillType;
                 skillRange = skillData.SkillRange;
                 skillDamage = skillData.SkillDamage;
                 skillCoolTime = skillData.SkillCoolTime;
                 projectilesNum = skillData.ProjectilesNum;
                 attackNum = skillData.AttackNum;
                 penetratingPower = skillData.PenetratingPower;
                 SkillDamageRange = skillData.SkillDamageRange;
                 effectID = skillData.EffectID.GetValueOrDefault();

                controller = skillData.AnimatorController;
                sprite = skillData.sprite;

                if (spriteRenderer != null && sprite != null)
                    spriteRenderer.sprite = sprite;

                if (animator != null && controller != null)
                    animator.runtimeAnimatorController = controller;
            }
        }
    }
    public void SetTarget(Vector3 target)
    {
        targetpos = target;
    }
}

