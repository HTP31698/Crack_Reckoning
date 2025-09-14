using JetBrains.Annotations;
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


    private NavMeshAgent agent;
    private float posX;
    private float posZ;

    public MonsterDataTable monsterDataTable;
    private MonsterData MonsterData;

    [SerializeField] private int id;

    private string monsterName;
    private int maxHp;
    private int currentHp;
    private int damage;
    private float attackSpeed;
    private MonsterWeakness monsterWeakness;
    private MonsterStrength monsterStrength;
    private int exp;
    private Sprite sprite;
    private RuntimeAnimatorController controller;


    private float lastAttackTime = 0f;

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
                damage = MonsterData.MonsterDamage;
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
                else if (controller == null)
                    Debug.LogError($"AnimatorController가 없습니다! ID:{id}");
            }
            else
            {
                Debug.LogError($"MonsterData가 없습니다! ID:{id}");
            }
        }
        else
        {
            Debug.LogError("MonsterDataTable이 null입니다!");
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