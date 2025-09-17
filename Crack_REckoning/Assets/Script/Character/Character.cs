using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";
    private static readonly string LevelUpTable = "LevelUpTable";
    private CharacterData characterData;
    private LevelUpData levelUpData;

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
        AddSkill(31002);
        AddSkill(31003);
        AddSkill(32004);
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
        Monster monster = MonsterManager.nearMonster(transform.position);
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
    private void UseSkill(Monster target, int index)
    {
        if (target != null)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Skill"),
                transform.position, Quaternion.identity);
            Skill skill = obj.GetComponent<Skill>();
            skill.Init(SkillIDs[index]);
            skill.SetTarget(target.transform.position);
            skill.SetCharacter(this, CharacterAttack, CharacterCri, CharacterCriDamage);
            switch (skill.AttackType)
            {
                case AttackTypeID.Projectile:
                    break;
                case AttackTypeID.Area:
                    skill.CastAreaDamage();
                    break;
            }
            Debug.Log($"{CharacterName}이(가) 스킬 {skill.SkillName} 사용!");
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
        }
    }

    public List<int> GetSkillIdList()
    {
        return SkillIDs;
    }
}