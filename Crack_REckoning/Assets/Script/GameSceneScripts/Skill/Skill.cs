using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 스킬 한 발(투사체/폭발/장판/레이저 등)을 담당하는 컴포넌트
/// - FX 프리팹은 InitFX()에서 '스킬 반경'에 정확히 맞도록 스케일
/// - 데미지 판정은 OverlapCircleAll + "중심이 반경 안" 필터로 '보이는 링'과 1:1 일치
/// </summary>
public class Skill : MonoBehaviour
{
    // ====== 상수/필드 ======
    private static readonly string SkillTableName = "SkillTable";
    private static readonly string MonsterLayerName = "Monster";

    public int skillID;

    private Vector3 targetpos;
    private Vector2 dir;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Character character;

    private SkillTable skillTable;
    private SkillData skillData;

    // === 데이터에서 들어오는 속성들 ===
    public int Id { get; private set; }
    public string SkillName { get; private set; }
    public SkillTypeID SkillTypeID { get; private set; }
    public float SkillRange { get; private set; }          // 레이저 길이 등
    public int SkillDamage { get; set; }
    public float SkillCoolTime { get; set; }
    public int ProjectilesNum { get; set; }
    public int AttackNum { get; set; }
    public int PenetratingPower { get; set; }
    public float SkillDamageRange { get; set; }            // 원형 영역/장판 반경
    public AttackTypeID AttackType { get; private set; }
    public string SkillDescription { get; set; }

    public float ExplosionRange { get; set; }              // 폭발 반경
    public float ExplosionDamage { get; set; }
    public float FreezeTime { get; set; }
    public float StunTime { get; set; }
    public float Duration { get; set; }
    public float PerSecond { get; set; }                   // DoT/레이저 틱 속도
    public float KonckBack { get; set; }
    public float Strain { get; set; }

    public float AuthorRadius { get; set; } = 1f;

    public Sprite sprite { get; private set; }
    public RuntimeAnimatorController controller { get; private set; }

    public float Speed = 5f;                               // 투사체 속도
    private int characterCri;
    private float characterCriDamage;

    private readonly HashSet<MonsterBase> alreadyHit = new(); // 같은 몬스터 중복타격 방지
    private bool areaFired = false;

    private GameObject particlePrefab;                     // 히트/폭발 FX 프리팹
    private Transform areaVisualRoot;                      // (선택) 장판 애니 전용 루트
    private float areaVisualBaseRadius = 1f;               // 루트=1일 때 기본 반경(자동 추정)

    private LineRenderer line;                             // 레이저용
    private float Elapsed = 0f;
    private float TickAcc = 0f;
    private bool laserStarted = false;
    private bool sphereStarted = false;

    private Material LaserMaterial;
    private GameObject sphereFX;                           // 도트 영역 FX 인스턴스
    private Vector2 sphereCenter;

    // ====== Unity 기본 생명주기 ======
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        line = GetComponent<LineRenderer>();
        if (line) line.enabled = false;

        // areaVisualBaseRadius 자동 추정(스프라이트 기준)
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
                CastProjectile(dt);
                break;

            case AttackTypeID.Area:
                CastArea();
                break;

            case AttackTypeID.Mine:
                break;

            case AttackTypeID.Laser:
                if (line) line.enabled = true;
                CastLaser(dt);
                break;

            case AttackTypeID.Explosion:
                CastExplosion(dt);
                break;

            case AttackTypeID.Haeil:
                break;

            case AttackTypeID.IceSheet:
            case AttackTypeID.BlackHole:
            case AttackTypeID.ElectricSphere:
                CastDotArea(dt);
                break;
        }
    }

    // ====== 초기화/데이터 바인딩 ======
    public void Init(int id)
    {
        Id = id;
        skillTable = DataTableManager.Get<SkillTable>(SkillTableName);
        InitSkillData();
    }

    public void InitWithData(int id, SkillData data)
    {
        Id = id;
        skillData = data;
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (data != null) ApplySkillData(id, data);
    }

    private void InitSkillData()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (skillTable != null)
        {
            skillData = skillTable.Get(Id);
            if (skillData != null) ApplySkillData(Id, skillData);
        }
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

        particlePrefab = data.SkillParticle;
        LaserMaterial = data.Material;
        controller = data.AnimatorController;
        sprite = data.sprite;

        AuthorRadius = data.AuthorRadius;

        if (spriteRenderer && sprite) spriteRenderer.sprite = sprite;
        if (animator && controller) animator.runtimeAnimatorController = controller;
    }

    public void SetCharacter(Character c, int cri, float criDmg)
    {
        character = c;
        characterCri = cri;
        characterCriDamage = criDmg;
    }

    public void SetTargetDirection(Vector2 direction)
    {
        dir = direction.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTargetPosition(Vector3 position) => targetpos = position;

    // ====== 핵심 1: FX를 '원하는 반경'에 정확히 맞추는 함수 ======
    // - desiredRadius: 이 스킬이 '지금' 보여줘야 하는 월드 반경(레벨업 반영)
    // - authorRadius : 이 프리팹이 localScale (1,1,1)일 때의 기준 반경(보통 1). 다르면 인자로 넘겨.
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

        // 위치(Z만 앞으로)
        var p = fx.transform.position;
        fx.transform.position = new Vector3(p.x, p.y, zFront);

        // 렌더러 정렬
        foreach (var r in fx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.sortingLayerName = sortingLayer;
            r.sortingOrder = sortingOrder;
        }
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;     // 로컬 기준
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;     // Transform 스케일 반영
            main.stopAction = ParticleSystemStopAction.None;
            main.gravityModifier = 0f;

            var em = ps.emission; em.enabled = true;
        }
        float s = desiredRadius / authorRadius;

        var baseScale = fx.transform.localScale;
        fx.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);

        foreach (var r in fx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (r.renderMode == ParticleSystemRenderMode.Stretch)
            {
                r.lengthScale *= s;
            }
        }

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            var trails = ps.trails;
            if (!trails.enabled) continue;

            var w = trails.widthOverTrail;
            switch (w.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    w.constant *= s; break;
                case ParticleSystemCurveMode.TwoConstants:
                    w.constantMin *= s; w.constantMax *= s; break;
                case ParticleSystemCurveMode.Curve:
                case ParticleSystemCurveMode.TwoCurves:
                    w.curveMultiplier *= s; break;
            }
            trails.widthOverTrail = w;
        }

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play();
    }

    // (선택) 장판 애니메이션 루트만 스케일하고 싶을 때 사용
    private void ApplyAreaAnimationScale(float radius)
    {
        radius = Mathf.Max(0.01f, radius);

        if (areaVisualBaseRadius <= 0.01f && spriteRenderer != null)
        {
            float approx = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * 0.5f;
            areaVisualBaseRadius = Mathf.Max(0.1f, approx);
        }

        float scale = radius / Mathf.Max(0.01f, areaVisualBaseRadius);
        var root = areaVisualRoot ? areaVisualRoot : transform;
        root.localScale = new Vector3(scale, scale, 1f);
    }

    // ====== 핵심 2: '보이는 반경'과 1:1로 맞게 판정하는 헬퍼 ======
    // - 콜라이더 중심이 원(center, radius) 안에 있어야 true
    private static bool IsInsideByCenter(Collider2D col, Vector2 center, float radius, float shrink = 0f)
    {
        float r = Mathf.Max(0f, radius - shrink);  // 살짝 보수적으로 쓰고 싶으면 shrink>0
        Vector2 c = col.bounds.center;             // 콜라이더 중심(피벗 편차 무시)
        return (c - center).sqrMagnitude <= r * r;
    }

    // ====== 데미지 계산 공통 ======
    private void TryAttack(MonsterBase m)
    {
        bool isCritical = Random.Range(0, 100) < characterCri;

        float typeMul = 1f;
        if (m.strength == SkillTypeID) typeMul = 0.5f;
        else if (m.weakness == SkillTypeID) typeMul = 1.5f;

        float dmg = SkillDamage * typeMul * (isCritical ? characterCriDamage : 1f);
        m.TakeDamage((int)dmg, character);
    }

    private void TryAttackExplosion(MonsterBase m, int baseDamage)
    {
        float typeMul = 1f;
        if (m.strength == SkillTypeID) typeMul = 0.5f;
        else if (m.weakness == SkillTypeID) typeMul = 1.5f;
        m.TakeDamage(Mathf.RoundToInt(baseDamage * typeMul), character);
    }

    private void TryAttackLaser(MonsterBase m, int dmg)
    {
        float typeMul = 1f;
        if (m.strength == SkillTypeID) typeMul = 0.5f;
        else if (m.weakness == SkillTypeID) typeMul = 1.5f;
        m.TakeDamage((int)(dmg * typeMul), character);
    }

    // ====== 각 공격 타입 구현 ======

    // 투사체: 전진 중 레이캐스트로 히트 체크, 히트 지점에 FX 생성
    public void CastProjectile(float dt)
    {
        Vector2 nextPos = rb.position + dir * Speed * dt;

        var hits = Physics2D.RaycastAll(rb.position, dir,
                                        (nextPos - rb.position).magnitude,
                                        LayerMask.GetMask(MonsterLayerName));
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (!m || m.isdead) continue;
            if (alreadyHit.Contains(m)) continue;
            alreadyHit.Add(m);

            TryAttack(m);

            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);

            if (particlePrefab)
            {
                var fx = Instantiate(particlePrefab, hitPos, Quaternion.identity);
                InitFX(fx, Mathf.Max(0.01f, SkillDamageRange), AuthorRadius);
            }

            PenetratingPower--;
            if (PenetratingPower <= 0) { Destroy(gameObject); break; }
        }

        rb.MovePosition(nextPos);

        if (nextPos.y > 7 || nextPos.y < -7 || nextPos.x > 10 || nextPos.x < -10)
            Destroy(gameObject);
    }

    // 즉발 원형: 타겟 지점에서 한 번만 터짐
    public void CastArea()
    {
        if (areaFired) return;
        areaFired = true;

        Vector2 center = targetpos;
        transform.SetPositionAndRotation(center, Quaternion.identity);

        float radius = Mathf.Max(0.01f, SkillDamageRange);
        ApplyAreaAnimationScale(radius);

        if (particlePrefab)
        {
            var fx = Instantiate(particlePrefab, center, Quaternion.identity);
            InitFX(fx, radius, AuthorRadius);
        }

        var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(MonsterLayerName));
        foreach (var c in cols)
        {
            var m = c.GetComponent<MonsterBase>();
            if (!m || m.isdead) continue;
            if (!IsInsideByCenter(c, center, radius)) continue; // 중심 필터
            TryAttack(m);
        }

        Destroy(gameObject, 1f);
    }

    // 레이저: LineRenderer 폭=SkillDamageRange, 길이=SkillRange
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

        if (dir == Vector2.zero) dir = Vector2.right; else dir = dir.normalized;

        Vector2 basePos = rb ? rb.position : (Vector2)transform.position;
        Vector2 origin = basePos + dir * 0.35f;
        float maxLen = Mathf.Max(0.01f, SkillRange);
        Vector3 endPos = origin + dir * maxLen;

        line.SetPosition(0, new Vector3(origin.x, origin.y, -0.1f));
        line.SetPosition(1, new Vector3(endPos.x, endPos.y, -0.1f));

        Elapsed += dt;
        if (Duration > 0f && Elapsed >= Duration) { StopAndDestroy(); return; }

        TickAcc += dt;
        float interval = (PerSecond > 0f) ? 1f / PerSecond : 0.2f;

        while (TickAcc >= interval)
        {
            TickAcc -= interval;
            float beamRadius = width * 0.5f;

            var hits = Physics2D.CircleCastAll(origin, beamRadius, dir, maxLen, LayerMask.GetMask(MonsterLayerName));
            foreach (var h in hits)
            {
                var m = h.collider ? h.collider.GetComponent<MonsterBase>() : null;
                if (!m || m.isdead) continue;
                TryAttackLaser(m, SkillDamage);
            }
        }
    }

    // 폭발: 히트 지점에서 원형으로 판정 + FX
    private void CastExplosion(float dt)
    {
        Vector2 ndir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;
        Vector2 nextPos = rb.position + ndir * Speed * dt;

        var hits = Physics2D.RaycastAll(
            rb.position,
            ndir, // ← dir 대신 ndir
            (nextPos - rb.position).magnitude,
            LayerMask.GetMask(MonsterLayerName)
        );
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (!m || m.isdead) continue;
            if (alreadyHit.Contains(m)) continue;
            alreadyHit.Add(m);

            // 폭발
            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);
            DoExplosion(hitPos);

            // FX: 폭발 반경에 딱 맞추기
            if (particlePrefab)
            {
                float r = Mathf.Max(0.01f, ExplosionRange);
                var fx = Instantiate(particlePrefab, hitPos, Quaternion.identity);
                InitFX(fx, r, AuthorRadius);
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
        float radius = Mathf.Max(0.01f, ExplosionRange);
        int dmg = (int)(SkillDamage * ExplosionDamage * (isCritical ? characterCriDamage : 1f));

        var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(MonsterLayerName));
        foreach (var c in cols)
        {
            var mob = c.GetComponent<MonsterBase>();
            if (!mob || mob.isdead) continue;

            // 중심이 반경 안일 때만 히트 → 보이는 링과 1:1 일치
            if (!IsInsideByCenter(c, center, radius /*, shrink: 0.0f*/)) continue;

            TryAttackExplosion(mob, dmg);
        }
    }

    // 지속 도트/블랙홀/얼음장판 등
    private void CastDotArea(float dt)
    {
        if (AttackType != AttackTypeID.BlackHole)
            if (spriteRenderer) spriteRenderer.enabled = false;

        if (!sphereStarted)
        {
            sphereStarted = true;
            Elapsed = 0f;
            TickAcc = 0f;

            sphereCenter = targetpos;
            transform.SetPositionAndRotation(sphereCenter, Quaternion.identity);

            float radius0 = Mathf.Max(0.01f, SkillDamageRange);
            ApplyAreaAnimationScale(radius0);

            if (particlePrefab)
            {
                sphereFX = Instantiate(particlePrefab, sphereCenter, Quaternion.identity);
                InitFX(sphereFX, radius0, AuthorRadius);
            }
        }

        Vector2 center = sphereCenter;
        float radius = Mathf.Max(0.01f, SkillDamageRange);

        Elapsed += dt;
        if (Duration > 0f && Elapsed >= Duration) { StopAndDestroy(); return; }

        TickAcc += dt;
        float interval = (PerSecond > 0f) ? 1f / PerSecond : 0.2f;

        while (TickAcc >= interval)
        {
            TickAcc -= interval;

            var cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask(MonsterLayerName));
            foreach (var c in cols)
            {
                var m = c.GetComponent<MonsterBase>();
                if (!m || m.isdead) continue;
                if (!IsInsideByCenter(c, center, radius /*, shrink: 0.0f*/)) continue;

                if (StunTime > 0f) m.ApplyStop(StunTime);
                if (FreezeTime > 0f) m.ApplyStop(FreezeTime);
                if (Strain > 0f) m.ApplyPool(sphereCenter, Strain);

                TryAttackLaser(m, SkillDamage);
            }
        }
    }

    // 유틸: 레이캐스트 히트 지점 계산
    private Vector3 GetHitPosition(RaycastHit2D hit, Rigidbody2D fromRb, Transform targetTf)
    {
        if (hit.point != Vector2.zero) return hit.point;

        var col2d = hit.collider as Collider2D;
        if (col2d != null) return col2d.ClosestPoint(fromRb.position);

        return targetTf ? targetTf.position : (Vector3)fromRb.position;
    }

    private void StopAndDestroy()
    {
        if (sphereFX)
        {
            Destroy(sphereFX);
        }
        if (line) line.enabled = false;
        Destroy(gameObject);
    }
}
