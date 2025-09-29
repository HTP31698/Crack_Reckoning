
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
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
    private Sprite TypeSprite;

    private SkillTable skillTable;
    private SkillData skillData;

    public int Id { get; private set; }
    public string SkillName { get; private set; }
    public SkillTypeID SkillTypeID { get; private set; }
    public float SkillRange { get; private set; }
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }
    public AttackTypeID AttackType { get; private set; }
    public string SkillDescription { get; set; }

    public float ExplosionRange { get; set; }
    public float ExplosionDamage { get; set; }
    public float FreezeTime { get; set; }
    public float StunTime { get; set; }
    public float Duration { get; set; }
    public float PerSecond { get; set; }
    public float KonckBack { get; set; }
    public float Strain { get; set; }

    public Sprite sprite { get; private set; }
    public RuntimeAnimatorController controller { get; private set; }

    public float Speed = 5f;
    private int characterCri;
    private float characterCriDamage;

    private readonly HashSet<MonsterBase> alreadyHit = new();

    private bool areaFired = false;

    private GameObject particlePrefab;
    private Transform areaVisualRoot;
    private float areaVisualBaseRadius = 1f;

    private LineRenderer line;

    private float Elapsed = 0f;
    private float TickAcc = 0f;
    private bool laserStarted = false;
    private bool sphereStarted = false;

    private Material LaserMaterial;
    private GameObject sphereFX;

    private Vector2 sphereCenter;
    private float monsterSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (!line) line = GetComponent<LineRenderer>();
        if (line) line.enabled = false;

        if (areaVisualBaseRadius <= 0.01f && spriteRenderer != null)
        {
            float approx = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * 0.5f;
            areaVisualBaseRadius = Mathf.Max(0.1f, approx);
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        switch (AttackType)
        {
            case AttackTypeID.Projectile:
                if (dir == Vector2.zero) return;
                CastProjectile(dt);   // ← 지금 쓰던 레이캐스트/관통 로직 그대로 이전
                break;
            case AttackTypeID.Area:
                CastArea();
                break;
            case AttackTypeID.Mine:

                break;

            case AttackTypeID.Laser:
                line.enabled = true;
                CastLaser(dt);
                break;
            case AttackTypeID.Explosion:
                CastExplosion(dt);
                break;
            case AttackTypeID.Haeil:
                break;
            case AttackTypeID.IceSheet:
                CastDotArea(dt);
                break;
            case AttackTypeID.BlackHole:
                CastDotArea(dt);
                break;
            case AttackTypeID.ElectricSphere:
                CastDotArea(dt);
                break;
        }
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

        float damage = SkillDamage
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
    private void InitFX(
     GameObject fx,
     float desiredRadius,
     float authorRadius = 1f,
     string sortingLayer = "Skill",
     int sortingOrder = 300,
     float zFront = -0.2f)
    {
        if (!fx) return;

        desiredRadius = Mathf.Max(0.01f, desiredRadius);
        authorRadius = Mathf.Max(0.01f, authorRadius);

        var p = fx.transform.position;
        fx.transform.position = new Vector3(p.x, p.y, zFront);

        foreach (var r in fx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.sortingLayerName = sortingLayer;
            r.sortingOrder = sortingOrder;
        }

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.stopAction = ParticleSystemStopAction.None;
            main.gravityModifier = 0f;

            var em = ps.emission;
            em.enabled = true;
        }

        float scale = desiredRadius / authorRadius;
        fx.transform.localScale = new Vector3(scale, scale, 1f);

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play();
    }

    private void ApplyAreaAnimationScale(float radius)
    {
        radius = Mathf.Max(0.01f, radius);

        // 기준 반경이 너무 작거나 0이면 sprite로 대략 추정
        if (areaVisualBaseRadius <= 0.01f && spriteRenderer != null)
        {
            float approx = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * 0.5f;
            areaVisualBaseRadius = Mathf.Max(0.1f, approx);
        }

        float scale = radius / Mathf.Max(0.01f, areaVisualBaseRadius);

        // 지정한 루트가 있으면 그 루트만 키우고, 없으면 전체를 키움
        var root = areaVisualRoot != null ? areaVisualRoot : transform;
        root.localScale = new Vector3(scale, scale, 1f);
    }
    private void ApplySkillData(int id, SkillData data)
    {
        Id = data.SkillID;
        SkillName = data.SkillName;
        SkillTypeID = data.SkillType;
        SkillRange = data.SkillRange;
        SkillDamage = data.SkillDamage;
        SkillCoolTime = data.SkillCoolTime;
        ProjectilesNum = data.ProjectilesNum;
        AttackNum = data.AttackNum;
        PenetratingPower = data.PenetratingPower;
        SkillDamageRange = data.SkillDamageRange;
        AttackType = data.AttackType;
        SkillDescription = data.SkillDescription;

        ExplosionRange = data.ExplosionRange.GetValueOrDefault();
        ExplosionDamage = data.ExplosionDamage.GetValueOrDefault();
        FreezeTime = data.FreezeTime.GetValueOrDefault();
        StunTime = data.StunTime.GetValueOrDefault();
        Duration = data.Duration.GetValueOrDefault();
        PerSecond = data.PerSecond.GetValueOrDefault();
        KonckBack = data.KonckBack.GetValueOrDefault();
        Strain = data.Strain.GetValueOrDefault();

        TypeSprite = data.TypeSprite;

        particlePrefab = data.SkillParticle;

        LaserMaterial = data.Material;

        // 비주얼 리소스
        controller = data.AnimatorController;
        sprite = data.sprite;


        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;
    }

    public void SetCharacter(Character character, int cri, float cridmg)
    {
        this.character = character;
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
    private Vector3 GetHitPosition(RaycastHit2D hit, Rigidbody2D fromRb, Transform targetTf)
    {
        if (hit.point != Vector2.zero)
            return hit.point;

        var col2d = hit.collider as Collider2D;
        if (col2d != null)
            return col2d.ClosestPoint(fromRb.position);

        return targetTf != null ? targetTf.position : fromRb.position;
    }

    //Cast1
    public void CastProjectile(float dt)
    {
        Vector2 nextPos = rb.position + dir * Speed * dt;
        var hits = Physics2D.RaycastAll(rb.position, dir,
                                        (nextPos - rb.position).magnitude,
                                        LayerMask.GetMask(Monster));
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (m == null || m.isdead) continue;

            if (alreadyHit.Contains(m)) continue;

            alreadyHit.Add(m);
            TryAttack(m);
            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);

            if (particlePrefab != null)
            {
                var fx = Instantiate(particlePrefab, hitPos, Quaternion.identity);
                InitFX(fx, Mathf.Max(0.01f, SkillDamageRange));
            }

            PenetratingPower--;
            if (PenetratingPower <= 0) { Destroy(gameObject); break; }
        }

        rb.MovePosition(nextPos);

        if (nextPos.y > 7 || nextPos.y < -7 || nextPos.x > 10 || nextPos.x < -10)
            Destroy(gameObject);
    }

    //Cast2
    public void CastArea()
    {
        if (areaFired)
        {
            return;
        }
        areaFired = true;

        Vector2 center = targetpos;
        gameObject.transform.position = targetpos;
        gameObject.transform.rotation = Quaternion.identity;

        float radius = Mathf.Max(0.01f, SkillDamageRange);
        ApplyAreaAnimationScale(radius);
        if (particlePrefab != null)
        {
            var fx = Instantiate(particlePrefab, center, Quaternion.identity);
            InitFX(fx, radius);
        }
        var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(Monster));
        foreach (var hit in cols)
        {
            MonsterBase m = hit.GetComponent<MonsterBase>();
            if (m == null || m.isdead) continue;
            TryAttack(m);
        }
        Destroy(gameObject, 1f);
    }

    //Cast3
    private void CastLaser(float dt)
    {
        if (!line) return;

        if (spriteRenderer) spriteRenderer.enabled = false;

        if (!laserStarted)
        {
            laserStarted = true;
            line.enabled = true;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            line.textureMode = LineTextureMode.Stretch;
            if (LaserMaterial) line.material = LaserMaterial;
            line.sortingOrder = 200;
        }

        float width = Mathf.Max(0.01f, SkillDamageRange);
        line.startWidth = width;
        line.endWidth = width;

        if (dir == Vector2.zero) dir = Vector2.right;
        else dir = dir.normalized;

        Vector2 basePos = rb ? rb.position : (Vector2)transform.position;
        Vector2 origin = basePos + dir * 0.35f;
        float maxLen = Mathf.Max(0.01f, SkillRange);
        Vector3 endPos = origin + dir * maxLen;

        line.SetPosition(0, new Vector3(origin.x, origin.y, -0.1f));
        line.SetPosition(1, new Vector3(endPos.x, endPos.y, -0.1f));

        Elapsed += dt;
        if (Duration > 0f && Elapsed >= Duration)
        { StopAndDestroy(); return; }

        TickAcc += dt;

        float interval;
        if (PerSecond > 0f)
        {
            interval = 1f / PerSecond;
        }
        else
        {
            interval = 0.2f;
        }
        while (TickAcc >= interval)
        {
            TickAcc -= interval;
            float beamRadius = width * 0.5f;
            var hits = Physics2D.CircleCastAll(
                origin,
                beamRadius,
                dir,
                maxLen,
                LayerMask.GetMask(Monster)
            );
            foreach (var h in hits)
            {
                var m = h.collider ? h.collider.GetComponent<MonsterBase>() : null;
                if (m == null || m.isdead) continue;
                TryAttackLaser(m, SkillDamage);
            }
        }
    }
    private void StopAndDestroy()
    {
        if (line)
            line.enabled = false;
        Destroy(gameObject);
    }

    private void TryAttackLaser(MonsterBase m, int dmg)
    {
        float typeMul = 1f;
        if (m.strength == SkillTypeID) typeMul = 0.5f;
        else if (m.weakness == SkillTypeID) typeMul = 1.5f;

        float damage = dmg * typeMul;
        m.TakeDamage((int)damage, character);
    }

    //Cast4
    private void CastExplosion(float dt)
    {
        Vector2 ndir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;

        Vector2 nextPos = rb.position + ndir * Speed * dt;

        var hits = Physics2D.RaycastAll(rb.position, dir,
                                        (nextPos - rb.position).magnitude,
                                        LayerMask.GetMask(Monster));
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (m == null || m.isdead) continue;

            if (alreadyHit.Contains(m)) continue;
            alreadyHit.Add(m);

            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);
            DoExplosion(hitPos);

            if (particlePrefab != null)
            {
                float r = ExplosionRange;
                var fx = Instantiate(particlePrefab, hitPos, Quaternion.identity);
                InitFX(fx, r);
            }
            PenetratingPower--;
            if (PenetratingPower <= 0) { Destroy(gameObject); break; }
        }
        rb.MovePosition(nextPos);

        if (nextPos.y > 7 || nextPos.y < -7 || nextPos.x > 10 || nextPos.x < -10)
            Destroy(gameObject);
    }
    private void DoExplosion(Vector2 center)
    {
        bool isCritical = Random.Range(0, 100) < characterCri;
        float radius = ExplosionRange;
        int dmg = (int)(SkillDamage * ExplosionDamage * (isCritical ? characterCriDamage : 1f));

        var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(Monster));
        foreach (var c in cols)
        {
            var mob = c.GetComponent<MonsterBase>();
            if (mob == null || mob.isdead) continue;
            TryAttackExplosion(mob, dmg);
        }
    }
    private void TryAttackExplosion(MonsterBase m, int baseDamage)
    {
        float typeMul = 1f;
        if (m.strength == SkillTypeID) typeMul = 0.5f;
        else if (m.weakness == SkillTypeID) typeMul = 1.5f;

        m.TakeDamage(Mathf.RoundToInt(baseDamage * typeMul), character);
    }

    //Cast5
    private void CastDotArea(float dt)
    {
        if(AttackType != AttackTypeID.BlackHole)
        {
            if (spriteRenderer) spriteRenderer.enabled = false;
        }

        if (!sphereStarted)
        {
            sphereStarted = true;

            Elapsed = 0f;
            TickAcc = 0f;

            sphereCenter = targetpos;

            transform.position = sphereCenter;
            transform.rotation = Quaternion.identity;

            float radius0 = Mathf.Max(0.01f, SkillDamageRange);
            ApplyAreaAnimationScale(radius0);

            if (particlePrefab != null)
            {
                sphereFX = Instantiate(particlePrefab, sphereCenter, Quaternion.identity);
                sphereFX.transform.SetParent(transform, true);
                InitFX(sphereFX, radius0);
            }
        }

        Vector2 center = sphereCenter;
        float radius = Mathf.Max(0.01f, SkillDamageRange);

        Elapsed += dt;
        if (Duration > 0f && Elapsed >= Duration)
        { StopAndDestroy(); return; }

        TickAcc += dt;

        float interval;
        if (PerSecond > 0f)
        {
            interval = 1f / PerSecond;
        }
        else
        {
            interval = 0.2f;
        }
        while (TickAcc >= interval)
        {
            TickAcc -= interval;
            var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(Monster));
            foreach (var c in cols)
            {
                var m = c.GetComponent<MonsterBase>();
                if (m == null || m.isdead) continue;
                if(StunTime > 0f)
                {
                    m.ApplyStop(StunTime);
                }
                if(FreezeTime > 0f)
                {
                    m.ApplyStop(FreezeTime);
                }
                if(Strain > 0f)
                {
                    m.ApplyPool(sphereCenter, Strain);
                }
                TryAttackLaser(m, SkillDamage);
            }
        }
    }
}
