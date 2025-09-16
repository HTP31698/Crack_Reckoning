using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

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
    }

    private void Update()
    {

    }

    //public void UseSkill(int id)
    //{
    //    Monster target = MonsterManager.nearMonster(gameObject.transform.position);
    //    if (target != null)
    //    {
    //        GameObject obj = Instantiate(skillprefaps,
    //            gameObject.transform.position, target.transform.rotation);
    //        Skill skill = obj.GetComponent<Skill>();
    //        skill.Init(id);
    //        skill.SetTarget(target.transform.position);

    //    }
    //}
}