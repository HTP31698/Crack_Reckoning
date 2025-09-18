using UnityEngine;

public class Monster : MonsterBase
{
    private static readonly string MonsterTable = "MonsterTable";
    private MonsterTable monsterTable;
    private MonsterData monsterData;
    private int exp;

    public Sprite getSprite { get; private set; }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(gameObject);
            Debug.Log("ÀÏ¹Ý ¸÷ »èÁ¦");
        }
    }

    public override void Init(int id)
    {
        this.id = id;
        monsterTable = DataTableManager.Get<MonsterTable>(MonsterTable);
        InitData();
    }

    protected override void InitData()
    {
        monsterData = monsterTable.Get(id);
        if (monsterData == null) return;

        monsterName = monsterData.MonsterName;
        agent.stoppingDistance = monsterData.MonsterRange;
        agent.speed = monsterData.MonsterSpeed;

        maxHp = monsterData.MonsterHp;
        currentHp = maxHp;
        damage = monsterData.MonsterAttack;
        attackSpeed = monsterData.MonsterAttackSpeed;
        exp = monsterData.MonsterExp;

        controller = monsterData.AnimatorController;
        sprite = monsterData.sprite;
        getSprite = monsterData.sprite;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;
        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;

        posX = transform.position.x;
        posZ = transform.position.z;
    }

    protected override void OnDeath(Character attacker)
    {
        attacker.AddExp(exp);
    }
}
