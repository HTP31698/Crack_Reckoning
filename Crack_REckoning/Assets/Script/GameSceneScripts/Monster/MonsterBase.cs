using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    protected Sprite sprite;
    protected RuntimeAnimatorController controller;

    protected float posX;
    protected float posZ;

    protected Coroutine attackCoroutine;

    public bool isdead { get; set; } = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
            attackCoroutine = StartCoroutine(AttackCoroutine(war));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackCoroutine(War target)
    {
        while (true)
        {
            target.TakeDamage(damage);
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.SetTrigger(Attack);
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
        if (currentHp <= 0)
        {
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


