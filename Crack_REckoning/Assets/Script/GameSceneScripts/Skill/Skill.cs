using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Skill : MonoBehaviour
{
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

    public int Id { get; private set; }
    public string SkillName { get; private set; }
    public SkillTypeID skillTypeID { get; private set; }
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

    public float AuthorRadius { get; set; } = 1f;
    public int SoundAttackID { get; set; }
    public int SoundHitID { get; set; }

    public Sprite sprite { get; private set; }
    public Sprite Typesprite { get; private set; }
    public RuntimeAnimatorController controller { get; private set; }

    public float Speed = 5f;
    private int characterCri;
    private float characterCriDamage;

    private readonly HashSet<MonsterBase> alreadyHit = new();
    private bool areaFired = false;

    private GameObject particlePrefab;
    private Transform areaVisualRoot = null;
    private float areaVisualBaseRadius = 1f;

    private LineRenderer line;
    private float Elapsed = 0f;
    private float TickAcc = 0f;
    private bool laserStarted = false;
    private bool sphereStarted = false;

    private Material LaserMaterial;
    private GameObject sphereFX;
    private Vector2 sphereCenter;

    private bool mineDetonated = false;
    public float MineDetonateAfter = 3f;
    private float beforeMinerTimer = 0f;
    private bool armed = false;
    private float armedTimer = 0f;

    public AudioSource audioSource;
    public AudioClip attackAudioClip;
    private AudioClip hitAudioClip;

    public bool skillend { get; private set; } = false;
    private int petdamage = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        line = GetComponent<LineRenderer>();
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
                CastProjectile(dt);
                break;
            case AttackTypeID.Area:
                CastArea();
                break;
            case AttackTypeID.Laser:
                if (line) line.enabled = true;
                CastLaser(dt);
                break;
            case AttackTypeID.Explosion:
                CastExplosion(dt);
                break;
            case AttackTypeID.ElectricSphere:
                CastDotArea(dt);
                break;
            case AttackTypeID.IceSheet:
                CastDotArea(dt);
                break;
            case AttackTypeID.BlackHole:
                CastDotArea(dt);
                break;
            case AttackTypeID.Haeil:
                CastProjectile(dt);
                break;
        }
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        if (AttackType == AttackTypeID.Mine)
        {
            CastMine(dt);
        }
    }

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
        skillTypeID = (SkillTypeID)data.SkillType;
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

        SoundAttackID = data.SoundAttackID;
        SoundHitID = data.SoundHitID;

        particlePrefab = data.SkillParticle;
        LaserMaterial = data.Material;
        controller = data.AnimatorController;
        sprite = data.sprite;
        Typesprite = data.TypeSprite;
        AuthorRadius = data.AuthorRadius;

        if(data.attackAudioClip != null)
        {
            attackAudioClip = data.attackAudioClip;
        }
        if (data.hitaudioClip != null)
        {
            hitAudioClip = data.hitaudioClip;
        }
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        if (animator != null && controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
    }

    public void SetCharacter(Character c, int cri, float criDmg, int damage)
    {
        character = c;
        characterCri = cri;
        characterCriDamage = criDmg;
        petdamage = damage;
    }

    public void SetTargetDirection(Vector2 direction)
    {
        dir = direction.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTargetPosition(Vector3 position) => targetpos = position;

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
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.useUnscaledTime = false;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.startDelay = 0f;
            main.loop = false;
            main.gravityModifier = 0f;

            var em = ps.emission; em.enabled = true;
            int n = em.burstCount;
            if (n > 0)
            {
                var bursts = new ParticleSystem.Burst[n];
                em.GetBursts(bursts);
                for (int i = 0; i < n; i++) bursts[i].time = 0f; // ★
                em.SetBursts(bursts);
            }
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

        float startOffsetSec = 1f;
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (startOffsetSec > 0f && AttackType == AttackTypeID.Mine)
                ps.Simulate(startOffsetSec, true, true, false);

            ps.Play();
        }
    }

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

    private static bool IsInsideByCenter(Collider2D col, Vector2 center, float radius, float shrink = 0f)
    {
        float r = Mathf.Max(0f, radius - shrink);
        Vector2 c = col.bounds.center;
        return (c - center).sqrMagnitude <= r * r;
    }

    private void TryAttack(MonsterBase m)
    {
        bool isCritical = Random.Range(0, 100) < characterCri;

        float typeMul = 1f;
        if (m.strength == skillTypeID) typeMul = 0.8f;
        else if (m.weakness == skillTypeID) typeMul = 1.3f;

        float dmg = SkillDamage * typeMul * (isCritical ? characterCriDamage : 1f);
        m.TakeDamage((int)dmg + petdamage, character);
        if (Time.timeScale > 0f && hitAudioClip)
            AudioSource.PlayClipAtPoint(hitAudioClip, transform.position, 1f);
    }

    private void TryAttackExplosion(MonsterBase m, int baseDamage)
    {
        float typeMul = 1f;
        if (m.strength == skillTypeID) typeMul = 0.8f;
        else if (m.weakness == skillTypeID) typeMul = 1.3f;
        m.TakeDamage(Mathf.RoundToInt(baseDamage * typeMul) + petdamage, character);
        if (Time.timeScale > 0f && hitAudioClip)
            AudioSource.PlayClipAtPoint(hitAudioClip, transform.position, 1f);
    }

    private void TryAttackLaser(MonsterBase m, int dmg)
    {
        bool isCritical = Random.Range(0, 100) < characterCri;

        float typeMul = 1f;
        if (m.strength == skillTypeID) typeMul = 0.8f;
        else if (m.weakness == skillTypeID) typeMul = 1.3f;
        m.TakeDamage((int)((dmg * typeMul) * (isCritical ? characterCriDamage : 1f)) + petdamage, character);
        if (Time.timeScale > 0f && hitAudioClip)
            AudioSource.PlayClipAtPoint(hitAudioClip, transform.position, 1f);
    }

    //Cast1
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

            if (KonckBack > 0f)
            {
                m.ApplyNuckBack(KonckBack);
            }

            TryAttack(m);
            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);

            if (particlePrefab)
            {
                var fx = Instantiate(particlePrefab, hitPos, Quaternion.identity);
                InitFX(fx, Mathf.Max(0.01f, ExplosionRange), AuthorRadius);
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
                if (particlePrefab)
                {
                    float r = Mathf.Max(0.01f, ExplosionRange);
                    var fx = Instantiate(particlePrefab, m.transform.position, Quaternion.identity);
                    InitFX(fx, ExplosionRange, AuthorRadius);
                }
                TryAttackLaser(m, SkillDamage);
            }
        }
    }

    //Cast4
    private void CastExplosion(float dt)
    {
        Vector2 ndir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.right;
        Vector2 nextPos = rb.position + ndir * Speed * dt;

        var hits = Physics2D.RaycastAll(
            rb.position,
            ndir,
            (nextPos - rb.position).magnitude,
            LayerMask.GetMask(MonsterLayerName)
        );
        foreach (var hit in hits)
        {
            var m = hit.collider.GetComponent<MonsterBase>();
            if (!m || m.isdead) continue;
            if (alreadyHit.Contains(m)) continue;
            alreadyHit.Add(m);

            Vector3 hitPos = GetHitPosition(hit, rb, m.transform);
            DoExplosion(hitPos);

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

            if (!IsInsideByCenter(c, center, radius)) continue;

            TryAttackExplosion(mob, dmg);
        }
    }

    //Cast5
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
                sphereFX = Instantiate(particlePrefab, sphereCenter, Quaternion.identity, transform);
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
    //Cast6
    private void CastMine(float dt)
    {
        if (mineDetonated)
            return;
        var center = (Vector2)transform.position;
        float detectRadius = Mathf.Max(0.01f, SkillDamageRange);
        ApplyAreaAnimationScale(detectRadius);
        beforeMinerTimer += dt;
        if (Duration > 0f && beforeMinerTimer > Duration)
        {
            StopAndDestroy();
            return;
        }
        bool hasLive = false;
        var cols = Physics2D.OverlapCircleAll(center, detectRadius, LayerMask.GetMask(MonsterLayerName));
        if (cols != null && cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                var m = cols[i].GetComponent<MonsterBase>();
                if (m != null && !m.isdead) { hasLive = true; break; }
            }
        }
        if (!armed)
        {
            if (hasLive)
            {
                armed = true;
                armedTimer = 0f;
            }
            return;
        }
        if (!hasLive)
        {
            armed = false;
            armedTimer = 0f;
            return;
        }
        armedTimer += dt;
        if (armedTimer >= MineDetonateAfter)
        {
            mineDetonated = true;
            if (particlePrefab)
            {
                var fx = Instantiate(particlePrefab, center, Quaternion.identity);
                InitFX(fx, Mathf.Max(0.01f, ExplosionRange), AuthorRadius);
                Destroy(fx, 1f);
            }
            DoExplosion(center);
            StopAndDestroy();
        }
    }


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
        if (line)
        {
            line.enabled = false;
        }
        Destroy(gameObject);
    }
}
