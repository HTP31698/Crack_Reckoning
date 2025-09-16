using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.U2D;

public class Skill : MonoBehaviour
{
    private static readonly string SkillTable = "SkillTable";

    public int skillID;
    private Vector3 targetpos;
    private Vector2 dir;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;


    private SkillTable skillTable;
    private SkillData skillData;

    public int Id { get; private set; }
    public string SkillName { get; private set; }
    public SkillSortationID SkillSortationID { get; private set; }
    public SkillTypeID SkillTypeID { get; private set; }
    public float SkillRange { get; private set; }
    public int SkillDamage { get; private set; }
    public float SkillCoolTime { get; private set; }
    public int ProjectilesNum { get; private set; }
    public int AttackNum { get; private set; }
    public int PenetratingPower { get; private set; }
    public float SkillDamageRange { get; private set; }
    public int EffectID { get; private set; }
    public AttackTypeID AttackType { get; private set; }

    public Sprite sprite { get; private set; }
    public RuntimeAnimatorController controller { get; private set; }

    public float Speed = 5f;
    private int characterAttack;
    private int characterCri;
    private float characterCriDamage;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer.sortingOrder = 2;
    }
    private void FixedUpdate()
    {
        if (dir != Vector2.zero && AttackType == AttackTypeID.Projectile)
        {
            Vector2 currentPos = rb.position;
            Vector2 nextPos = currentPos + dir * Speed * Time.fixedDeltaTime;

            RaycastHit2D hit = Physics2D.Raycast(currentPos, dir, (nextPos - currentPos).magnitude, LayerMask.GetMask("Monster"));
            if (hit.collider != null)
            {
                Monster m = hit.collider.GetComponent<Monster>();
                if (m != null)
                {
                    TryAttack(m);
                    Destroy(gameObject);
                    return;
                }
            }

            rb.MovePosition(nextPos);
            if(nextPos.y > 7)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
    private void TryAttack(Monster m)
    {
        int rand = Random.Range(0, 100);
        if (rand < characterCri)
        {
            float cri = (SkillDamage + characterAttack) * characterCriDamage;
            m.TakeDamage((int)cri);
            Destroy(gameObject);
        }
        else
        {
            int nocri = SkillDamage + characterAttack;
            m.TakeDamage(nocri);
            Destroy(gameObject);
        }
    }

    public void Init(int id)
    {
        this.Id = id;
        skillTable = DataTableManager.Get<SkillTable>(SkillTable);
        InitSkillData();
    }
    private void InitSkillData()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (skillTable != null)
        {
            skillData = skillTable.Get(Id);
            if (skillData != null)
            {
                Id = skillData.SkillID;
                SkillName = skillData.SkillName;
                SkillSortationID = skillData.SkillSortation;
                SkillTypeID = skillData.SkillType;
                SkillRange = skillData.SkillRange;
                SkillDamage = skillData.SkillDamage;
                SkillCoolTime = skillData.SkillCoolTime;
                ProjectilesNum = skillData.ProjectilesNum;
                AttackNum = skillData.AttackNum;
                PenetratingPower = skillData.PenetratingPower;
                SkillDamageRange = skillData.SkillDamageRange;
                EffectID = skillData.EffectID.GetValueOrDefault();
                AttackType = skillData.AttackType;

                controller = skillData.AnimatorController;
                sprite = skillData.sprite;

                if (spriteRenderer != null && sprite != null)
                    spriteRenderer.sprite = sprite;

                if (animator != null && controller != null)
                    animator.runtimeAnimatorController = controller;
            }
        }
    }

    public void SetCharacter(int atk, int cri, float cridmg)
    {
        characterAttack = atk;
        characterCri = cri;
        characterCriDamage = cridmg;
    }
    public void SetTarget(Vector3 target)
    {
        targetpos = target;
        dir = ((Vector2)targetpos - rb.position).normalized;
        if(AttackType == AttackTypeID.Projectile)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    public void CastAreaDamage()
    {
        Vector2 damagePosition = (Vector2)transform.position + new Vector2(0, -spriteRenderer.bounds.size.y / 2);
        gameObject.transform.position = targetpos;
        Collider2D[] hits = Physics2D.OverlapCircleAll(damagePosition, SkillDamageRange, LayerMask.GetMask("Monster"));
        foreach (var hit in hits)
        {
            Monster m = hit.GetComponent<Monster>();
            if (m != null)
                m.TakeDamage(SkillDamage); // 필요하면 캐릭터 공격력/크리 적용
        }
        Destroy(gameObject, 1f);
    }
}

