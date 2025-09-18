using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";
    private static readonly string LevelUpTable = "LevelUpTable";

    private CharacterData characterData;
    private LevelUpData levelUpData;

    public GameManager gm;

    // 내부 스킬 ID 리스트
    private List<int> SkillIDs;

    // 스킬 준비 상태
    private List<bool> skillReady;

    // 캐릭터 기본 속성
    public int Id { get; private set; }
    public string CharacterName { get; private set; }
    public int BasicSkill { get; private set; }
    public int CharacterAttack { get; private set; }
    public int CharacterCri { get; private set; }
    public float CharacterCriDamage { get; private set; }

    public int skillActiveCount { get; private set; } = 0;
    private int level = 1;
    public int expToNextLevel { get; private set; }
    private int currentExp = 0;

    public Slider expSlider;

    private void Awake()
    {
        SkillIDs = new List<int>();
        skillReady = new List<bool>();
        for (int i = 0; i < 5; i++)
            skillReady.Add(false);

        Init(11001); // 기본 캐릭터 초기화
        AddSkill(BasicSkill);
    }

    private void Update()
    {
        // 모든 준비된 스킬 자동 사용
        for (int i = 0; i < SkillIDs.Count; i++)
        {
            if (i < skillReady.Count && skillReady[i])
                StartCoroutine(AutoUseSkill(i));
        }
    }

    public void Init(int id)
    {
        this.Id = id;

        var table = DataTableManager.Get<CharacterTable>(CharacterTable);
        characterData = table.Get(id);

        if (characterData != null)
        {
            CharacterName = characterData.ChName;
            BasicSkill = characterData.BasicSkill;
            CharacterAttack = characterData.ChAttack;
            CharacterCri = characterData.ChCri;
            CharacterCriDamage = characterData.ChCriDam;
        }

        ExpToNextLevel(level);
        if (expSlider != null)
            expSlider.value = (float)currentExp / expToNextLevel;
    }

    public void ExpToNextLevel(int level)
    {
        var leveltable = DataTableManager.Get<LevelUpTable>(LevelUpTable);
        levelUpData = leveltable.Get(level);
        if (levelUpData != null)
        {
            expToNextLevel = levelUpData.LvExp;
        }
    }

    // 내부 SkillIDs 복사본 반환
    public List<int> GetSkillIdList()
    {
        return new List<int>(SkillIDs);
    }

    public void AddSkill(int newSkillId)
    {
        if (skillActiveCount < 5)
        {
            SkillIDs.Add(newSkillId);
            skillReady[skillActiveCount] = true;
            ++skillActiveCount;
        }
    }

    public void IncreaseSkill(int skillId, SkillSelectionData skillData)
    {
        var skillTable = DataTableManager.Get<SkillTable>("SkillTable");
        SkillData s = skillTable.Get(skillId);
        if (s == null) return;

        if (skillData.SkillDamageNumChange > 0)
            s.SkillDamage = (int)(s.SkillDamage * skillData.SkillDamageNumChange);

        if (skillData.IncreasingSkillDamageRange.GetValueOrDefault() > 0)
            s.SkillDamageRange += skillData.IncreasingSkillDamageRange.Value;

        if (skillData.ReduceSkillCT.GetValueOrDefault() > 0)
            s.SkillCoolTime = Mathf.Max(0.1f, s.SkillCoolTime - skillData.ReduceSkillCT.Value);

        if (skillData.IncreasedProjectile.GetValueOrDefault() > 0)
            s.ProjectilesNum += skillData.IncreasedProjectile.Value;

        if (skillData.IncreaseNumAttack.GetValueOrDefault() > 0)
            s.AttackNum += skillData.IncreaseNumAttack.Value;

        if (skillData.PeneTratingPower.GetValueOrDefault() > 0)
            s.PenetratingPower += skillData.PeneTratingPower.Value;
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
        if (expSlider != null)
            expSlider.value = (float)currentExp / expToNextLevel;
    }

    public void LevelUp()
    {
        if (level >= 30) return;

        level++;
        ExpToNextLevel(level);
        if (gm != null)
            gm.ShowLevelUpSkills();
    }

    // ----------------- 스킬 자동 사용 -----------------
    private IEnumerator AutoUseSkill(int index)
    {
        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);
        if (skillData == null) yield break;

        // 가까운 몬스터 선택
        MonsterBase target = MonsterManager.GetRandomMonster(); // 랜덤 타겟
        if (target == null) yield break;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= skillData.SkillRange)
        {
            skillReady[index] = false;

            for (int atk = 0; atk < skillData.AttackNum; atk++)
            {
                UseSkill(target, index);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(skillData.SkillCoolTime);
            skillReady[index] = true;
        }
    }

    private void UseSkill(MonsterBase target, int index)
    {
        if (target == null) return;

        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);
        if (skillData == null) return;

        // 사거리 내 모든 몬스터
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skillData.SkillRange, LayerMask.GetMask("Monster"));
        List<MonsterBase> validTargets = new List<MonsterBase>();
        foreach (var hit in hits)
        {
            MonsterBase m = hit.GetComponent<MonsterBase>();
            if (m != null && !m.isdead)
                validTargets.Add(m);
        }

        if (validTargets.Count == 0)
            validTargets.Add(target);

        List<MonsterBase> tempTargets = new List<MonsterBase>(validTargets);

        for (int i = 0; i < skillData.ProjectilesNum; i++)
        {
            MonsterBase chosenTarget = null;
            if (tempTargets.Count > 0)
            {
                int randIndex = Random.Range(0, tempTargets.Count);
                chosenTarget = tempTargets[randIndex];
                tempTargets.RemoveAt(randIndex);
            }

            Vector3 spawnPos = transform.position + new Vector3((i - (skillData.ProjectilesNum - 1) / 2f) * 0.2f, 0, 0);
            GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Skill"), spawnPos, Quaternion.identity);
            Skill skill = obj.GetComponent<Skill>();
            skill.Init(SkillIDs[index]);
            skill.SetCharacter(this, CharacterAttack, CharacterCri, CharacterCriDamage);

            Vector3 targetPos = chosenTarget != null ? chosenTarget.transform.position : target.transform.position;
            Vector2 dir = ((Vector2)targetPos - (Vector2)spawnPos).normalized;
            skill.SetTargetPosition(targetPos);
            skill.SetTargetDirection(dir);

            if (skill.AttackType == AttackTypeID.Area)
                skill.CastAreaDamage();
        }
    }
}
