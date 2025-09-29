using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class MonsterBase : MonoBehaviour
{
    protected static readonly string Attack = "Attack";
    protected static readonly string IsDead = "IsDead";

    [SerializeField] protected Transform target;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected NavMeshAgent agent;

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

    public bool isdead { get; set; } = false;
    public bool isattack { get; set; } = false;

    private bool isStunned = false;
    private bool isStrain = false;
    private float stunRemain = 0f;
    private float strainRemain = 0f;
    private float cachedSpeed = -1f;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sliderHp = GetComponentInChildren<Slider>();

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
    }
    protected virtual void FixedUpdate()
    {
        if (target == null) return;
        if (isStunned) return;
        Vector3 movepos = new Vector3(posX, target.position.y + agent.stoppingDistance, posZ);
        if(!isStrain)
        {
            agent.SetDestination(movepos);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        War war = collision.GetComponent<War>();
        if (war == null) return;

        if (attackCoroutine == null)
        {
            isattack = true;
            attackCoroutine = StartCoroutine(AttackCoroutine(war));
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isattack = false;
    }

    private IEnumerator AttackCoroutine(War target)
    {
        while (true)
        {
            if (!isStunned)
            {
                target.TakeDamage(damage);
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.SetTrigger(Attack);
                }
            }
            yield return new WaitForSeconds(attackSpeed);
        }
    }
    public void SetTarget(Transform Target)
    {
        target = Target;
    }

    public void TakeDamage(int amount, Character attacker)
    {
        currentHp -= amount;
        sliderHp.value = (float)currentHp / (float)maxHp;
        if (currentHp <= 0)
        {
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
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isattack = false;

        StartCoroutine(DestroyGameObject());
    }

    private IEnumerator DestroyGameObject()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
        Destroy(gameObject);
    }

    public abstract void Init(int id);


    protected abstract void InitData();
    protected abstract void OnDeath(Character attacker);
}


