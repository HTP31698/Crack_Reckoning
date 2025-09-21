using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class Skill : MonoBehaviour
{
    private static readonly string SkillTable = "SkillTable";
    private static readonly string Monster = "Monster";

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

    private readonly HashSet<MonsterBase> alreadyHit = new(); // ★ 이 발사체가 이미 때린 적들

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (AttackType != AttackTypeID.Projectile || dir == Vector2.zero) 
            return;

        Vector2 nextPos = rb.position + dir * Speed * Time.fixedDeltaTime;
        var hits = Physics2D.RaycastAll(rb.position, dir,
                                        (nextPos - rb.position).magnitude,
                                        LayerMask.GetMask(Monster));
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (m == null || m.isdead) continue;

            // ★ 같은 몬스터는 한 번만 타격
            if (alreadyHit.Contains(m)) continue;

            alreadyHit.Add(m);     // 기록
            TryAttack(m);          // 1회만 데미지
            PenetratingPower--;    // 관통력 1 소모

            if (PenetratingPower <= 0) { Destroy(gameObject); break; }
        }

        rb.MovePosition(nextPos);

        // 화면 밖 정리
        if (nextPos.y > 7 || nextPos.y < -7 || nextPos.x > 10 || nextPos.x < -10)
            Destroy(gameObject);
    }

    private void TryAttack(MonsterBase m)
    {
        bool isCritical = Random.Range(0, 100) < characterCri;
        float typeMultiplier = 1f;
        if (m.strength == SkillTypeID)
        {
            typeMultiplier = 0.5f;
        }
        else if (m.weakness == SkillTypeID)
        {
            typeMultiplier = 1.5f;
        }

        float damage = (SkillDamage + characterAttack)
                       * typeMultiplier
                       * (isCritical ? characterCriDamage : 1f);

        m.TakeDamage((int)damage, character);
        Debug.Log($"몬스터 피: {m.currentHp}데미지 배율 : {typeMultiplier} " +
            $"/데미지: {damage} /크리:{isCritical} / 스킬 이름:{SkillName}");
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
                ApplySkillData(Id, skillData);
            }
        }
    }

    public void InitWithData(int id, SkillData data)
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        this.Id = id;
        this.skillData = data;
        if (data != null)
        {
            ApplySkillData(id, data);
        }
    }

    private void ApplySkillData(int id, SkillData data)
    {
        Id = data.SkillID;
        SkillName = data.SkillName;
        SkillSortationID = data.SkillSortation;
        SkillTypeID = data.SkillType;
        SkillRange = data.SkillRange;
        SkillDamage = data.SkillDamage;
        SkillCoolTime = data.SkillCoolTime;
        ProjectilesNum = data.ProjectilesNum;
        AttackNum = data.AttackNum;
        PenetratingPower = data.PenetratingPower;
        SkillDamageRange = data.SkillDamageRange;
        EffectID = data.EffectID.GetValueOrDefault();
        AttackType = data.AttackType;

        // 비주얼 리소스
        controller = data.AnimatorController;
        sprite = data.sprite;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;
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
        gameObject.transform.rotation = Quaternion.identity;

        Vector2 damagePosition = (Vector2)transform.position + new Vector2(0, -spriteRenderer.bounds.size.y * 0.9f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            damagePosition,
            SkillDamageRange,
            LayerMask.GetMask(Monster)
        );

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
