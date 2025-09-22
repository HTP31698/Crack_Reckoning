using UnityEngine;

public class Boss : MonsterBase
{
    private static readonly string BossTable = "BossTable";
    private BossTable bossTable;
    private BossData bossData;
    public bool IsBoss { get; set; }

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
        IsBoss = bossData.IsBoss;
        weakness = bossData.BossWeakness.GetValueOrDefault();
        strength = bossData.BossStrength.GetValueOrDefault();

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
        if (IsBoss)
        {
            attacker.LevelUp();
        }
    }
}
