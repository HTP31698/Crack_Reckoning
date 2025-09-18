using UnityEngine;

public class Boss : MonsterBase
{
    private static readonly string BossTable = "BossTable";
    private BossTable bossTable;
    private BossData bossData;
    private bool IsMiniBoss;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            Destroy(gameObject);
            Debug.Log("보스 몹 삭제");
        }
    }
    public override void Init(int id)
    {
        this.id = id;
        bossTable = DataTableManager.Get<BossTable>(BossTable);
        InitData();
    }

    protected override void InitData()
    {
        bossData = bossTable.Get(id);
        if (bossData == null) return;

        monsterName = bossData.BossName;
        agent.stoppingDistance = bossData.BossRange;
        agent.speed = bossData.BossSpeed;

        maxHp = bossData.BossHp;
        currentHp = maxHp;
        damage = bossData.BossAttack;
        attackSpeed = bossData.BossAttackSpeed;
        IsMiniBoss = bossData.IsMiniBoss;

        // TODO: Boss 전용 애니메이터, 스프라이트 할당
        controller = bossData.AnimatorController;
        sprite = bossData.sprite;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;
        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;

        posX = transform.position.x;
        posZ = transform.position.z;
    }

    protected override void OnDeath(Character attacker)
    {
        if (IsMiniBoss)
        {
            attacker.LevelUp();
        }
    }
}
