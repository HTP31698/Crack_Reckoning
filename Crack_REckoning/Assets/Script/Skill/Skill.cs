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
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }
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
        if (AttackType != AttackTypeID.Projectile || dir == Vector2.zero) return;

        Vector2 nextPos = rb.position + dir * Speed * Time.fixedDeltaTime;
        RaycastHit2D[] hits = Physics2D.RaycastAll(rb.position, dir, (nextPos - rb.position).magnitude, LayerMask.GetMask("Monster"));

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

        // 화면 밖이나 조건 충족 시 파괴
        if (nextPos.y > 7 || nextPos.y < -7 || nextPos.x > 10 || nextPos.x < -10)
            Destroy(gameObject);
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
    public void SetTargetDirection(Vector2 direction)
    {
        dir = direction.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetpos = position;
    }
    public void CastAreaDamage()
    {
        gameObject.transform.position = targetpos;
        gameObject.transform.rotation = Quaternion .identity;
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

