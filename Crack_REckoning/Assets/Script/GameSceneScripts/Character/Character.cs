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

    public bool isUseSkill { get; set; } = false;

    private void Awake()
    {
        SkillIDs = new List<int>();
        skillReady = new List<bool>();
        for (int i = 0; i < 5; i++)
            skillReady.Add(false);

        Skillpre = Resources.Load<GameObject>(SkillPrefabs);

        Init(11001);
        PlaySetting.PlayerBasicSkillID = BasicSkill;
        AddSkill(31001);
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

        bool anyInRange = false;

        // 1) 지정 반경(skillData.SkillRange) 안의 모든 Collider2D 가져오기
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,          // 원의 중심 = 내 위치
            skillData.SkillRange,        // 원의 반지름 = 스킬 사거리
            LayerMask.GetMask("Monster") // "Monster" 레이어만 검사
        );
        foreach (Collider2D col in hits)
        {
            // collider에 MonsterBase 컴포넌트가 붙어있는지 찾기
            MonsterBase m = col.GetComponent<MonsterBase>();

            // m이 실제 몬스터이고 죽지 않았다면
            if (m != null && !m.isdead)
            {
                anyInRange = true; // 살아있는 몬스터 발견!
                break;            // 하나만 찾으면 되니까 바로 반복 종료
            }
        }
        if (!anyInRange) yield break;

        // 최우선: 사거리 내 공격 중 몬스터
        MonsterBase priority = MonsterManager.GetAttackingWithin(transform.position, skillData.SkillRange);

        // 폴백: 기존 랜덤
        MonsterBase target = priority ?? MonsterManager.GetRandomMonster();
        if (target == null) yield break;

        skillReady[index] = false;
        isUseSkill = true;
        for (int atk = 0; atk < skillData.AttackNum; atk++)
        {
            UseSkill(target, index, priority); // 우선 타깃을 넘겨줌
            yield return new WaitForSeconds(0.2f);
        }


        yield return new WaitForSeconds(skillData.SkillCoolTime);
        isUseSkill = false;
        skillReady[index] = true;
    }

    private void UseSkill(MonsterBase target, int index, MonsterBase priorityTarget)
    {
        if (target == null) return;

        int skillId = SkillIDs[index];
        var skillData = GetSkillForUse(skillId);
        if (skillData == null) return;

        var chosenThisSkill = new HashSet<MonsterBase>();

        for (int i = 0; i < skillData.ProjectilesNum; i++)
        {
            MonsterBase chosen = null;

            if (i == 0 && priorityTarget != null && !priorityTarget.isdead)
            {
                // 첫 발은 반드시 우선 타깃(= 공격 중인 몬스터)
                chosen = priorityTarget;
            }
            else
            {
                // 이후 발사체는 기존 분산 로직
                chosen = MonsterManager.GetRandomAliveWithin(
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
            }

            Vector3 spawnPos = transform.position
                             + new Vector3((i - (skillData.ProjectilesNum - 1) / 2f) * 0.2f, 0, 0);

            GameObject obj = Instantiate(Skillpre, spawnPos, Quaternion.identity);
            Skill skill = obj.GetComponent<Skill>();
            if (skill != null)
            {
                skill.InitWithData(skillId, skillData);
                skill.SetCharacter(this, CharacterAttack, CharacterCri, CharacterCriDamage);

                // 널 안전 + 우선 타깃/선택 타깃 좌표
                Vector3 targetPos = (chosen != null ? chosen.transform.position : target.transform.position);

                // (원래 하던) 방향 계산
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