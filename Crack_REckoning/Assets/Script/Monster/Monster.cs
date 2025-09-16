using JetBrains.Annotations;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private static readonly string Attack = "Attack";
    private static readonly string die = "Die";

    [SerializeField]
    public Transform target;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private War war;

    private NavMeshAgent agent;
    private float posX;
    private float posZ;

    private MonsterDataTable monsterDataTable;
    private MonsterData MonsterData;

    [SerializeField] private int id;

    public string monsterName;
    public int maxHp;
    public int currentHp;
    public int damage;
    public float attackSpeed;
    public float monsterLastAttack = 0;
    private MonsterWeakness monsterWeakness;
    private MonsterStrength monsterStrength;
    public int exp;
    private Sprite sprite;
    private RuntimeAnimatorController controller;


    private float lastAttackTime = 0f;

    private void OnEnable()
    {
        MonsterManager.AddMonster(this);
    }
    private void OnDisable()
    {
        MonsterManager.RemoveMonster(this);
    }

    public void Init(MonsterDataTable table, int id)
    {
        monsterDataTable = table;
        this.id = id;
        InitMonsterData();
    }
    private void InitMonsterData()
    {
        // 컴포넌트가 null이면 강제로 가져오기
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (monsterDataTable != null)
        {
            MonsterData = monsterDataTable.Get(id);
            if (MonsterData != null)
            {
                monsterName = MonsterData.MonsterName;

                if (agent != null)
                {
                    agent.stoppingDistance = MonsterData.MonsterRange;
                    agent.speed = MonsterData.MonsterSpeed;
                }

                maxHp = MonsterData.MonsterHp;
                currentHp = maxHp;
                damage = MonsterData.MonsterAttack;
                attackSpeed = MonsterData.MonsterAttackSpeed;
                monsterWeakness = (MonsterWeakness)MonsterData.MonsterWeakness;
                monsterStrength = (MonsterStrength)MonsterData.MonsterStrength;
                exp = MonsterData.MonsterExp;

                controller = MonsterData.AnimatorController;
                sprite = MonsterData.sprite;

                if (spriteRenderer != null && sprite != null)
                    spriteRenderer.sprite = sprite;

                if (animator != null && controller != null)
                    animator.runtimeAnimatorController = controller;
            }
        }

        posX = transform.position.x;
        posZ = transform.position.z;
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (target == null) return;
        //Target Move
        Vector3 movepos = new Vector3(posX, target.position.y + agent.stoppingDistance, posZ);
        agent.SetDestination(movepos);

        //Target Attack
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    TryAttack();
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("War"))
            StartCoroutine(AttackCoroutine(collision.GetComponent<War>()));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("War"))
            StopCoroutine(AttackCoroutine(collision.GetComponent<War>()));
    }

    private IEnumerator AttackCoroutine(War target)
    {
        while (true)
        {
            target.TakeDamage(damage);
            yield return new WaitForSeconds(attackSpeed);
        }
    }


    private void TryAttack()
    {
        if(Time.time - lastAttackTime >= attackSpeed)
        {
            animator.SetTrigger(Attack);
            //add WarDamage
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        if(currentHp <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        animator.SetTrigger(die);
        agent.isStopped = true;

        //add player Exp

        Destroy(gameObject);
    }

    public void SetTarget (Transform Target)
    {
        target = Target;
    }
}