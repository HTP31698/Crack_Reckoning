using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";
    private static readonly string LevelUpTable = "LevelUpTable";
    private CharacterData characterData;
    private LevelUpData levelUpData;
    public GameManager gm;

    private List<bool> skillReady;

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

    private float distance;

    private List<int> SkillIDs;
    public TextMeshProUGUI text;

    private void Awake()
    {
        //Init(PlaySetting.playerID);
        Init(11001);
        SkillIDs = new List<int>();

        skillReady = new List<bool>();
        for (int i = 0; i < 5; i++)
            skillReady.Add(false);

        AddSkill(BasicSkill);
    }

    private void Update()
    {
        for (int i = 0; i < SkillIDs.Count; i++)
        {
            if (skillReady[i])
                StartCoroutine(AutoUseSkill(i));
        }
    }

    public void Init(int id)
    {
        this.Id = id;
        var table = DataTableManager.Get<CharacterTable>(CharacterTable);
        ExpToNextLevel(level);

        characterData = table.Get(id);
        text.text = $"{currentExp} / {expToNextLevel}\nlevel : {level}";

        if (characterData != null)
        {
            CharacterName = characterData.ChName;
            BasicSkill = characterData.BasicSkill;
            CharacterAttack = characterData.ChAttack;
            CharacterCri = characterData.ChCri;
            CharacterCriDamage = characterData.ChCriDam;
        }
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
    private IEnumerator AutoUseSkill(int index)
    {
        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);
        MonsterBase monster = MonsterManager.nearMonster(transform.position);
        if (monster != null)
        {
            distance = Vector3.Distance(transform.position, monster.transform.position);
            if (skillData.SkillRange >= distance)
            {
                skillReady[index] = false;
                UseSkill(monster, index);
                yield return new WaitForSeconds(skillData.SkillCoolTime);
                skillReady[index] = true;
            }
        }
    }
    private void UseSkill(MonsterBase target, int index)
    {
        if (target == null) return;

        // 스킬 데이터 (강화 포함)
        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);

        // 사거리 내 모든 몬스터 가져오기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skillData.SkillRange, LayerMask.GetMask("Monster"));
        List<MonsterBase> validTargets = new List<MonsterBase>();
        foreach (var hit in hits)
        {
            MonsterBase m = hit.GetComponent<MonsterBase>();
            if (m != null && !m.isdead)
                validTargets.Add(m);
        }

        // 발사할 투사체 수
        int projectileCount = Mathf.Min(skillData.ProjectilesNum, validTargets.Count);

        // 이미 선택된 타겟 저장 (중복 방지)
        HashSet<MonsterBase> usedTargets = new HashSet<MonsterBase>();

        for (int i = 0; i < projectileCount; i++)
        {
            // 랜덤 타겟 선택 (중복 제거)
            MonsterBase chosenTarget = null;
            if (validTargets.Count > 0)
            {
                int randIndex = Random.Range(0, validTargets.Count);
                chosenTarget = validTargets[randIndex];
                validTargets.RemoveAt(randIndex); // 중복 방지
            }

            // 투사체 생성
            GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Skill"),
                transform.position, Quaternion.identity);

            Skill skill = obj.GetComponent<Skill>();
            skill.Init(SkillIDs[index]);
            skill.SetCharacter(this, CharacterAttack, CharacterCri, CharacterCriDamage);

            if (chosenTarget != null)
                skill.SetTarget(chosenTarget.transform.position);
            else
                skill.SetTarget(target.transform.position); // fallback (없으면 원래 타겟)

            // 강화 수치 적용
            skill.ProjectilesNum = skillData.ProjectilesNum;
            skill.PenetratingPower = skillData.PenetratingPower;

            if (skill.AttackType == AttackTypeID.Area)
                skill.CastAreaDamage();
        }
    }


    public void AddSkill(int newSkillId)
    {
        if (skillActiveCount < 5)
        {
            SkillIDs.Add(newSkillId);
            skillReady[skillActiveCount] = true;
            ++skillActiveCount;
        }
        else
        {
            return;
        }
    }
    public void AddExp(int amount)
    {
        currentExp += amount;
        while(currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
        text.text = $"{currentExp} / {expToNextLevel}\nlevel : {level}";
    }

    public void LevelUp()
    {
        if (level > 30)
        {
            level = 30;
            return;
        }
        else
        {
            level++;
            ExpToNextLevel(level);
            gm.ShowLevelUpSkills();
        }
    }

    public List<int> GetSkillIdList()
    {
        return SkillIDs;
    }

    public void IncreaseSkill(int skillId, SkillSelectionData skillData)
    {
        // 현재 스킬 데이터를 가져옴
        var skillTable = DataTableManager.Get<SkillTable>("SkillTable");
        SkillData s = skillTable.Get(skillId);

        if (s == null) return;

        // 데미지
        if (skillData.SkillDamageNumChange > 0)
            s.SkillDamage = (int)(s.SkillDamage * skillData.SkillDamageNumChange);

        // 범위
        if (skillData.IncreasingSkillDamageRange.HasValue)
            s.SkillDamageRange += skillData.IncreasingSkillDamageRange.Value;

        // 쿨타임
        if (skillData.ReduceSkillCT.HasValue)
            s.SkillCoolTime = Mathf.Max(0.1f, s.SkillCoolTime - skillData.ReduceSkillCT.Value);

        // 투사체 수
        if (skillData.IncreasedProjectile.HasValue)
            s.ProjectilesNum += skillData.IncreasedProjectile.Value;

        // 공격 횟수
        if (skillData.IncreaseNumAttack.HasValue)
            s.AttackNum += skillData.IncreaseNumAttack.Value;

        // 관통력
        if (skillData.PeneTratingPower.HasValue)
            s.PenetratingPower += skillData.PeneTratingPower.Value;

        Debug.Log($"스킬 {s.SkillName} 강화 적용 완료!");
    }
}