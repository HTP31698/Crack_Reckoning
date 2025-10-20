
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public abstract class MonsterBase : MonoBehaviour
{
    protected static readonly string Attack = "Attack";
    protected static readonly string IsDead = "IsDead";

    [SerializeField] protected Transform target;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected NavMeshAgent agent;
    protected AudioSource audioSource;

    protected int id;
    public string monsterName;
    public int maxHp;
    public int currentHp;
    public int damage;
    public float attackSpeed;
    protected float lastAttackTime = 0f;

    protected Slider sliderHp;

    public SkillTypeID weakness;
    public SkillTypeID strength;
    protected Sprite sprite;
    protected RuntimeAnimatorController controller;

    protected float posX;
    protected float posZ;

    protected Coroutine attackCoroutine;

    public bool isdead { get; protected set; } = false;
    public bool isattack { get; protected set; } = false;

    private bool isStunned = false;
    private bool isStrain = false;
    private float stunRemain = 0f;
    private float strainRemain = 0f;
    private float cachedSpeed = -1f;

    // ★ stoppingDistance 히스테리시스(너무 잦은 on/off 방지)
    [SerializeField] private float startEpsilon = 0.01f; // 시작 여유
    [SerializeField] private float stopEpsilon = 0.05f; // 정지 여유(조금 더 크게)
    private float meleeRangeFallback = 0.5f; // stoppingDistance가 0일 때 최소 근접 판정 거리

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sliderHp = GetComponentInChildren<Slider>();
        audioSource = GetComponent<AudioSource>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void OnEnable() => MonsterManager.AddMonster(this);
    private void OnDisable() => MonsterManager.RemoveMonster(this);

    protected virtual void Update()
    {
        if (isStunned)
        {
            stunRemain -= Time.deltaTime;
            if (stunRemain <= 0f)
            {
                isStunned = false;
                stunRemain = 0f;

                if (agent != null)
                {
                    agent.isStopped = false;
                    if (cachedSpeed >= 0f) agent.speed = cachedSpeed;
                }
            }
            if (isStrain)
            {
                isStunned = false;
                stunRemain = 0f;

                if (agent != null)
                {
                    agent.isStopped = false;
                    if (cachedSpeed >= 0f) agent.speed = cachedSpeed;
                }
            }
        }
        if (isStrain)
        {
            strainRemain -= Time.deltaTime;
            if (strainRemain <= 0f)
            {
                strainRemain = 0f;
                isStrain = false;
            }
        }

        if (!isStunned && target != null)
        {
            bool inRangeToStart = IsWithinStopDistance(startEpsilon);
            bool inRangeToStay = IsWithinStopDistance(-stopEpsilon) || inRangeToStart;
            if (attackCoroutine == null && inRangeToStart)
            {
                var war = target.GetComponent<War>();
                if (war != null) StartAttack(war);
            }
            else if (attackCoroutine != null && !inRangeToStay)
            {
                StopAttack();
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (target == null) return;
        if (isStunned) return;

        Vector3 movepos = new Vector3(posX, target.position.y + agent.stoppingDistance, posZ);
        if (!isStrain)
        {
            agent.SetDestination(movepos);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        var war = collision.GetComponent<War>();
        if (war == null) return;

        if (IsWithinStopDistance(startEpsilon))
            StartAttack(war);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsWithinStopDistance(-stopEpsilon))
            StopAttack();
    }

    private IEnumerator AttackCoroutine(War targetWar)
    {
        float period = Mathf.Max(0.05f, attackSpeed); // 0/음수 방지
        while (true)
        {
            if (!isStunned)
            {
                // ★ 실제 데미지도 stoppingDistance 기준으로 게이팅
                if (IsWithinStopDistance(0f) && targetWar != null)
                {
                    targetWar.TakeDamage(damage);
                }

                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.SetTrigger(Attack);
                }
            }
            yield return new WaitForSeconds(period);
        }
    }

    public void SetTarget(Transform Target) => target = Target;

    public void TakeDamage(int amount, Character attacker)
    {
        currentHp -= amount;
        sliderHp.value = (float)currentHp / (float)maxHp;
        if (currentHp <= 0)
        {
            audioSource.Play();
            currentHp = 0;
            sliderHp.value = (float)currentHp / (float)maxHp;
            OnDeath(attacker);
            Die();
        }
    }

    public void ApplyStop(float duration)
    {
        if (duration <= 0f || agent == null) return;

        if (!isStunned)
        {
            cachedSpeed = agent.speed;
            isStunned = true;

            agent.speed = 0f;
            agent.isStopped = true;
            if (isStrain)
            {
                isStunned = false;
            }
        }
        stunRemain = Mathf.Max(stunRemain, duration);
    }

    public void ApplyPool(Vector2 pos, float duration)
    {
        if (duration <= 0f || agent == null) return;
        if (!isStrain)
        {
            isStrain = true;
            agent.SetDestination(pos);
        }
        strainRemain = Mathf.Max(strainRemain, duration);
    }

    public void ApplyNuckBack(float nuckback)
    {
       agent.gameObject.transform.position = new Vector3(posX, gameObject.transform.position.y + nuckback, posZ); 
    }

    public virtual void Die()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(IsDead);
        isdead = true;
        agent.isStopped = true;
        isStunned = false;
        isStrain = false;
        stunRemain = 0f;
        strainRemain = 0f;
        if (agent)
        {
            agent.isStopped = true;
            if (cachedSpeed >= 0f)
            {
                agent.speed = cachedSpeed;
            }
        }
        StopAttack();
        StartCoroutine(DestroyGameObject());
    }

    private IEnumerator DestroyGameObject()
    {
        float wait = (animator != null) ? animator.GetCurrentAnimatorStateInfo(0).length * 0.5f : 0.3f;
        yield return new WaitForSeconds(wait);
        Destroy(gameObject);
    }

    public abstract void Init(int id);
    protected abstract void InitData();
    protected abstract void OnDeath(Character attacker);

    private bool IsWithinStopDistance(float extra)
    {
        if (agent == null || target == null) return false;

        float sd = agent.stoppingDistance > 0f ? agent.stoppingDistance : meleeRangeFallback;
        float range = Mathf.Max(0f, sd + extra);

        if (agent.updateUpAxis == false)
        {
            return Mathf.Abs(transform.position.y - target.position.y) <= range;
        }
        else
        {
            return Mathf.Abs(transform.position.z - target.position.z) <= range;
        }
    }


    private void StartAttack(War war)
    {
        if (war == null || isStunned) return;
        if (attackCoroutine != null) return;

        isattack = true;
        attackCoroutine = StartCoroutine(AttackCoroutine(war));
    }

    private void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isattack = false;
    }
}



