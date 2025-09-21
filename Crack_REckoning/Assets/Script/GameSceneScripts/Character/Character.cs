using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";
    private static readonly string LevelUpTable = "LevelUpTable";
    private static readonly string SkillTable = "SkillTable";
    private static readonly string SkillPrefabs = "Prefabs/Skill";

    private GameObject Skillpre;

    private CharacterData characterData;
    private LevelUpData levelUpData;

    public GameManager gm;

    private List<int> SkillIDs;

    private List<bool> skillReady;

    private readonly Dictionary<int, SkillData> skillInstances = new();

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

        Skillpre = Resources.Load<GameObject>(SkillPrefabs);

        Init(11001); 
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
        if (!skillInstances.TryGetValue(skillId, out var s))
        {
            var skillTable = DataTableManager.Get<SkillTable>(SkillTable);
            var def = skillTable?.Get(skillId);
            if (def == null) return;

            s = def.Clone();
            skillInstances[skillId] = s;
        }

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

    private SkillData GetSkillForUse(int skillId)
    {
        if (skillInstances.TryGetValue(skillId, out var inst))
            return inst;

        return DataTableManager.Get<SkillTable>(SkillTable)?.Get(skillId);
    }

    private IEnumerator AutoUseSkill(int index)
    {
        int skillId = SkillIDs[index];
        var skillData = GetSkillForUse(skillId);
        if (skillData == null) yield break;

        // ▼ 사거리 안에 몬스터가 한 마리라도 있으면 발사
        bool anyInRange = Physics2D.OverlapCircleAll(
            transform.position,
            skillData.SkillRange,
            LayerMask.GetMask("Monster")
        ).Any(col =>
        {
            var m = col.GetComponent<MonsterBase>();
            return m != null && !m.isdead;
        });

        if (!anyInRange)
            yield break;

        // 발사는 여전히 "랜덤 타겟" 기준
        MonsterBase target = MonsterManager.GetRandomMonster();
        if (target == null) yield break;

        skillReady[index] = false;

        for (int atk = 0; atk < skillData.AttackNum; atk++)
        {
            UseSkill(target, index);   // ← 발사체마다 사거리 내에서 랜덤 타겟 뽑음
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(skillData.SkillCoolTime);
        skillReady[index] = true;
    }

    private void UseSkill(MonsterBase target, int index)
    {
        if (target == null) return;

        int skillId = SkillIDs[index];
        var skillData = GetSkillForUse(skillId);
        if (skillData == null) return;

        var chosenThisSkill = new HashSet<MonsterBase>();

        for (int i = 0; i < skillData.ProjectilesNum; i++)
        {
            MonsterBase chosen = MonsterManager.GetRandomAliveWithin(
                transform.position,
                skillData.SkillRange,
                chosenThisSkill
            );

            if (chosen == null)
            {
                chosen = MonsterManager.GetRandomAliveWithin(
                    transform.position,
                    skillData.SkillRange,
                    null
                );
            }

            if (chosen == null) chosen = target;

            Vector3 spawnPos = transform.position
                             + new Vector3((i - (skillData.ProjectilesNum - 1) / 2f) * 0.2f, 0, 0);

            GameObject obj = Instantiate(Skillpre, spawnPos, Quaternion.identity);
            Skill skill = obj.GetComponent<Skill>();
            if (skill != null)
            {
                // 강화가 반영된 내 사본 데이터 사용
                skill.InitWithData(skillId, skillData);
                skill.SetCharacter(this, CharacterAttack, CharacterCri, CharacterCriDamage);

                Vector3 targetPos = (chosen != null ? chosen.transform.position : target.transform.position);
                Vector2 dir = ((Vector2)targetPos - (Vector2)spawnPos).normalized;

                skill.SetTargetPosition(targetPos);
                skill.SetTargetDirection(dir);

                if (skill.AttackType == AttackTypeID.Area)
                    skill.CastAreaDamage();
            }

            if (chosen != null)
                chosenThisSkill.Add(chosen);
        }
    }
}