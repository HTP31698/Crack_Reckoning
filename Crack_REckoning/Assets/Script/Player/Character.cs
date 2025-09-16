using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

    public GameObject skillprefaps;
    private int skillCount;

    private CharacterData characterData;
    private SkillData skillData;
    private SkillSelectionData skillSelectionData;

    private List<int> ids = new List<int>();
    private string characterName;
    private int basicSkill;
    private int newSkillID;
    private int characterAttack;
    private int characterCri;
    private float characterCriDamage;



    private void Awake()
    {
        AddSkill(basicSkill);
        skillCount = 1;
    }

    private void Update()
    {
        //IEnumerator Coroutine으로 나중에 시간마다 스킬 사용하게끔? Add로 스킬 추가하고
        if (Input.GetKeyDown(KeyCode.R))
        {

            UseSkill(basicSkill);
        }
        if (Input.GetKeyDown(KeyCode.W) && skillCount < 5)
        {
            skillCount++;
            AddSkill(basicSkill);
        }
    }

    public void UseSkill(int id)
    {
        Monster target = MonsterManager.nearMonster(gameObject.transform.position);
        if (target != null)
        {
            GameObject obj = Instantiate(skillprefaps,
                gameObject.transform.position, target.transform.rotation);
            Skill skill = obj.GetComponent<Skill>();
            skill.Init(id);
            skill.SetTarget(target.transform.position);

        }
    }

    public void AddSkill(int id)
    {
        ids.Add(id);
    }
}