using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
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
    private Character character;


    private SkillTable skillTable;
    private SkillData skillData;

    public int Id { get; private set; }
    public string SkillName { get; private set; }
    public SkillSortationID SkillSortationID { get; private set; }
    public SkillTypeID SkillTypeID { get; private set; }
    public float SkillRange { get; private set; }
    public int SkillDamage { get;  set; }
    public float SkillCoolTime { get;  set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get;  set; }
    public int PenetratingPower { get;  set; }
    public float SkillDamageRange { get;  set; }
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
    }
    private void FixedUpdate()
    {
        if (AttackType != AttackTypeID.Projectile) return;

        if (dir != Vector2.zero)
        {
            for (int i = 0; i < ProjectilesNum; i++)
            {
                Vector2 spawnOffset = new Vector2(i * 0.2f, 0); // 투사체 위치 약간씩 차이나게
                Vector2 currentPos = rb.position + spawnOffset;
                Vector2 nextPos = currentPos + dir * Speed * Time.fixedDeltaTime;

                RaycastHit2D[] hits = Physics2D.RaycastAll(currentPos, dir, (nextPos - currentPos).magnitude, LayerMask.GetMask("Monster"));
                foreach (var hit in hits)
                {
                    MonsterBase m = hit.collider.GetComponent<MonsterBase>();
                    if (m != null && !m.isdead)
                    {
                        TryAttack(m);
                        PenetratingPower--;
                        if (PenetratingPower <= 0) Destroy(gameObject);
                    }
                }
                rb.MovePosition(nextPos);

                if (nextPos.y > 7)
                    Destroy(gameObject);
            }
        }
    }

    private void TryAttack(MonsterBase m)
    {
        int rand = Random.Range(0, 100);
        if (rand < characterCri)
        {
            float cri = (SkillDamage + characterAttack) * characterCriDamage;
            m.TakeDamage((int)cri, character);
        }
        else
        {
            int nocri = SkillDamage + characterAttack;
            m.TakeDamage(nocri, character);
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

    public void SetCharacter(Character character, int atk, int cri, float cridmg)
    {
        this.character = character;
        characterAttack = atk;
        characterCri = cri;
        characterCriDamage = cridmg;
    }
    public void SetTarget(Vector3 target)
    {
        targetpos = target;
        if (AttackType == AttackTypeID.Projectile)
        {
            dir = ((Vector2)targetpos - rb.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            dir = Vector2.zero;
        }
    }
    public void CastAreaDamage()
    {
        gameObject.transform.position = targetpos;
        Vector2 damagePosition = (Vector2)transform.position + new Vector2(0, -spriteRenderer.bounds.size.y * 0.9f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(damagePosition, SkillDamageRange, LayerMask.GetMask("Monster"));
        foreach (var hit in hits)
        {
            MonsterBase m = hit.GetComponent<MonsterBase>();
            if (m != null)
            {
                TryAttack(m);
            }
        }
        Destroy(gameObject, 1f);
    }
}

