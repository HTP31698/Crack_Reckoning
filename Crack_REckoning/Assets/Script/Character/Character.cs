using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
    public Slider expSlider;

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
        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelUp();
        }
    }

    public void Init(int id)
    {
        this.Id = id;
        var table = DataTableManager.Get<CharacterTable>(CharacterTable);
        ExpToNextLevel(level);

        characterData = table.Get(id);
        expSlider.value = (float)currentExp/expToNextLevel;
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
            float distance = Vector3.Distance(transform.position, monster.transform.position);

            if (skillData.SkillRange >= distance)
            {
                skillReady[index] = false;

                for (int atk = 0; atk < skillData.AttackNum; atk++)
                {
                    UseSkill(monster, index);
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForSeconds(skillData.SkillCoolTime);
                skillReady[index] = true;
            }
        }
    }

    private void UseSkill(MonsterBase target, int index)
    {
        if (target == null) return;

        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);

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
        {
            validTargets.Add(target);
        }


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

            float spread = 0.2f;
            Vector3 spawnPos = transform.position + new Vector3((i - (skillData.ProjectilesNum - 1) / 2f) * spread, 0, 0);

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
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
        expSlider.value = (float)currentExp / expToNextLevel;
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
        var skillTable = DataTableManager.Get<SkillTable>("SkillTable");
        SkillData s = skillTable.Get(skillId);

        if (s == null) return;

        // 데미지
        if (skillData.SkillDamageNumChange > 0)
        {
            s.SkillDamage = (int)(s.SkillDamage * skillData.SkillDamageNumChange);
        }
        // 범위
        if (skillData.IncreasingSkillDamageRange.GetValueOrDefault() > 0)
        {
            s.SkillDamageRange += skillData.IncreasingSkillDamageRange.Value;
        }
        // 쿨타임
        if (skillData.ReduceSkillCT.GetValueOrDefault() > 0)
        {
            s.SkillCoolTime = Mathf.Max(0.1f, s.SkillCoolTime - skillData.ReduceSkillCT.Value);
        }
        // 투사체 수
        if (skillData.IncreasedProjectile.GetValueOrDefault() > 0)
        {
            s.ProjectilesNum += skillData.IncreasedProjectile.Value;
        }
        // 공격 횟수
        if (skillData.IncreaseNumAttack.GetValueOrDefault() > 0)
        {
            s.AttackNum += skillData.IncreaseNumAttack.Value;
        }
        // 관통력
        if (skillData.PeneTratingPower.GetValueOrDefault() > 0)
        {
            s.PenetratingPower += skillData.PeneTratingPower.Value;
        }
    }
}