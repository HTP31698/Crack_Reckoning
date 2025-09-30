using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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

    private AudioSource audioSource;

    private readonly Dictionary<int, SkillData> skillInstances = new();

    // 캐릭터 기본 속성
    public int Id { get; private set; }
    public string CharacterName { get; private set; }
    public int CharacterCri { get; private set; }
    public float CharacterCriDamage { get; private set; }

    public string ChDesc { get; private set; }

    public int skillActiveCount { get; private set; } = 0;
    private int level = 1;
    public int expToNextLevel { get; private set; }
    private int currentExp = 0;

    public Slider expSlider;
    public bool isUseSkill { get; set; } = false;

    private Sprite sprite;

    private SpriteRenderer spriteRenderer;

    private Animator animator;

    private RuntimeAnimatorController runtimeAnimatorController;

    public Pet pet;

    private void Awake()
    {
        SkillIDs = new List<int>();
        skillReady = new List<bool>();
        for (int i = 0; i < 5; i++)
            skillReady.Add(false);
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Skillpre = Resources.Load<GameObject>(SkillPrefabs);

        var data = SaveLoadManager.Data;
        Init(data.PlayerID);

        if (data != null && data.EquipmentSkillIds != null)
        {
            foreach (var id in data.EquipmentSkillIds)
            {
                if (id > 0) AddSkill(id);
            }
        }
    }

    private void Update()
    {
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
            CharacterCri = characterData.ChCri;
            CharacterCriDamage = characterData.ChCriDam;
            ChDesc = characterData.ChDesc;

            sprite = characterData.sprite;
            runtimeAnimatorController = characterData.runanimator;

            if (runtimeAnimatorController != null & animator != null)
            {
                animator.runtimeAnimatorController = runtimeAnimatorController;
            }
            if (sprite != null && spriteRenderer)
            {
                spriteRenderer.sprite = sprite;
            }
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

        if (skillData.Duration.GetValueOrDefault() > 0)
        {
            s.Duration += skillData.Duration.Value;
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            skillData.SkillRange,
            LayerMask.GetMask("Monster")
        );
        foreach (Collider2D col in hits)
        {
            MonsterBase m = col.GetComponent<MonsterBase>();
            if (m != null && !m.isdead)
            {
                anyInRange = true;
                break;
            }
        }
        if (!anyInRange) yield break;

        MonsterBase priority = MonsterManager.GetAttackingWithin(transform.position, skillData.SkillRange);
        skillReady[index] = false;
        isUseSkill = true;
        for (int atk = 0; atk < skillData.AttackNum; atk++)
        {
            UseSkill(index, priority);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(skillData.SkillCoolTime);
        isUseSkill = false;
        skillReady[index] = true;
    }

    private void UseSkill(int index, MonsterBase priorityTarget = null)
    {
        int skillId = SkillIDs[index];
        var skillData = GetSkillForUse(skillId);
        if (skillData == null) return;

        var chosenThisSkill = new HashSet<MonsterBase>();

        for (int i = 0; i < skillData.ProjectilesNum; i++)
        {
            MonsterBase chosen = null;

            if (i == 0 && priorityTarget != null && !priorityTarget.isdead)
            {
                if (Vector2.Distance(transform.position, priorityTarget.transform.position) <= skillData.SkillRange)
                    chosen = priorityTarget;
            }

            if (chosen == null)
            {
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

                if (chosen == null)
                {
                    chosen = MonsterManager.GetRandomMonster();
                }
            }

            if (chosen == null) continue;

            Vector3 spawnPos = transform.position
                             + new Vector3((i - (skillData.ProjectilesNum - 1) / 2f) * 0.2f, 0, 0);
            Vector3 targetPos = chosen.transform.position;
            Vector2 dir = ((Vector2)targetPos - (Vector2)spawnPos).normalized;

            Vector3 haeilPos = new Vector3(targetPos.x, spawnPos.y, targetPos.z);
            Vector2 haeildir = ((Vector2)targetPos - (Vector2)haeilPos).normalized;

            GameObject obj = Instantiate(Skillpre, spawnPos, Quaternion.identity);
            Skill skill = obj.GetComponent<Skill>();

            if (skill != null)
            {
                skill.InitWithData(skillId, skillData);
                skill.SetTargetPosition(targetPos);
                skill.SetTargetDirection(dir);
                skill.SetCharacter(this, CharacterCri, CharacterCriDamage, pet.GetAttBuff());

                audioSource.spatialBlend = 0f;
                audioSource.priority = 32;
                audioSource.PlayOneShot(skill.attackAudioClip, 1f);

                if (skill.AttackType == AttackTypeID.Haeil)
                {
                    obj.gameObject.transform.position = haeilPos;
                    skill.SetTargetDirection(haeildir);
                }
                if (skill.AttackType == AttackTypeID.Mine)
                {
                    obj.gameObject.transform.position = RandomInBoxThenClampToSafe(obj);
                }


            }
            chosenThisSkill.Add(chosen);
        }
    }
    private static Vector3 RandomInBoxThenClampToSafe(
     GameObject prefab, float zDepth = 0f, float marginPct = 0.05f)
    {
        // 1) 박스에서 먼저 랜덤 (요구 범위)
        float rawX = Random.Range(-2.7f, 2.7f);
        float rawY = Random.Range(-1.5f, 4.0f);

        var cam = Camera.main;
        if (!cam) return new Vector3(rawX, rawY, zDepth);

        // 2) 안전영역 월드 사각형
        Rect safe = Screen.safeArea;
        float zDist = Mathf.Abs(cam.transform.position.z - zDepth);
        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(safe.xMin, safe.yMin, zDist));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(safe.xMax, safe.yMax, zDist));

        // 3) 프리팹 반폭/반높이(루트 스케일 기준)
        float halfW = 0f, halfH = 0f;
        if (prefab)
        {
            var sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr && sr.sprite)
            {
                Vector3 s = prefab.transform.localScale;
                halfW = sr.sprite.bounds.extents.x * Mathf.Abs(s.x);
                halfH = sr.sprite.bounds.extents.y * Mathf.Abs(s.y);
            }
        }

        // 4) 여백 반영 + 클램프 범위 계산
        float marginX = (tr.x - bl.x) * Mathf.Clamp01(marginPct);
        float marginY = (tr.y - bl.y) * Mathf.Clamp01(marginPct);


        float randhx = Random.Range(0.25f, 0.5f);

        float minX = bl.x + halfW * 0.5f + marginX;
        float maxX = tr.x - halfW * 0.5f - marginX;
        float minY = bl.y + halfH * 0.5f + marginY;
        float maxY = tr.y - halfH * 0.5f - marginY;

        // 5) 안전장치: 범위가 너무 좁으면 중앙 기준 최소 폭 확보
        const float kMinSpan = 0.05f;
        if (maxX - minX < kMinSpan) { float mid = (bl.x + tr.x) * 0.5f; minX = mid - kMinSpan * 0.5f; maxX = mid + kMinSpan * 0.5f; }
        if (maxY - minY < kMinSpan) { float mid = (bl.y + tr.y) * 0.5f; minY = mid - kMinSpan * 0.5f; maxY = mid + kMinSpan * 0.5f; }

        // 6) ‘안으로 끌어들이기’
        float x = Mathf.Clamp(rawX, minX, maxX);
        float y = Mathf.Clamp(rawY, minY, maxY);

        return new Vector3(x, y, zDepth);
    }



}