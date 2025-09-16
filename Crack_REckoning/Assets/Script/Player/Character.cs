using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

    private CharacterData characterData;

    private List<bool> skillReady;

    public int Id { get; private set; }
    public string CharacterName { get; private set; }
    public int BasicSkill { get; private set; }
    public int CharacterAttack { get; private set; }
    public int CharacterCri { get; private set; }
    public float CharacterCriDamage { get; private set; }

    private List<int> SkillIDs;

    private void Awake()
    {
        Init(11001);

        SkillIDs = new List<int>();
        SkillIDs.Add(BasicSkill);
        SkillIDs.Add(32004);

        // ��ų ���� �ʱ�ȭ
        skillReady = new List<bool>();
        for (int i = 0; i < SkillIDs.Count; i++)
            skillReady.Add(true);
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
        characterData = table.Get(id);

        if (characterData != null)
        {
            CharacterName = characterData.ChName;
            BasicSkill = characterData.BasicSkill;
            CharacterAttack = characterData.ChAttack;
            CharacterCri = characterData.ChCri;
            CharacterCriDamage = characterData.ChCriDam;

            Debug.Log($"ĳ���� �ε� �Ϸ�: {CharacterName}, �⺻��ų: {BasicSkill}");
        }
        else
        {
            Debug.LogError($"CharacterTable���� ID {id} �����͸� ã�� �� �����ϴ�.");
        }
    }
    private IEnumerator AutoUseSkill(int index)
    {
        skillReady[index] = false;
        UseSkill(index);
        var skillData = DataTableManager.Get<SkillTable>("SkillTable").Get(SkillIDs[index]);

        yield return new WaitForSeconds(skillData.SkillCoolTime);

        skillReady[index] = true;
    }
    private void UseSkill(int index)
    {
        Monster target = MonsterManager.nearMonster(transform.position);

        if (target != null)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Skill"),
                transform.position, Quaternion.identity);

            Skill skill = obj.GetComponent<Skill>();
            skill.Init(SkillIDs[index]);
            skill.SetTarget(target.transform.position);
            skill.SetCharacter(CharacterAttack, CharacterCri, CharacterCriDamage);
            
            switch(skill.AttackType)
            {
                case AttackTypeID.Projectile:
                    break;
                case AttackTypeID.Area:
                    skill.CastAreaDamage();
                    break;
            }
            Debug.Log($"{CharacterName}��(��) ��ų {skill.SkillName} ���!");
        }
    }


    public void AddSkill(int newSkillId)
    {
        SkillIDs.Add(newSkillId);
    }
}