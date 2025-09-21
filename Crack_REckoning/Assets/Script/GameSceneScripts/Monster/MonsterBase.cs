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

    protected virtual void FixedUpdate()
    {
        if (target == null) return;
        Vector3 movepos = new Vector3(posX, target.position.y + agent.stoppingDistance, posZ);
        agent.SetDestination(movepos);
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
            target.TakeDamage(damage);
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger(Attack);
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

    public virtual void Die()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(IsDead);
        isdead = true;
        agent.isStopped = true;

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


