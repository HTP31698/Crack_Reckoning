using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

    public GameObject skillprefaps;
    private int skillCount = 1;

    private CharacterData characterData;
    private SkillData skillData;
    private SkillSelectionData skillSelectionData;

    private int[] ids = new int[5];
    private string characterName;
    private int basicSkill;
    private int characterAttack;
    private int characterCri;
    private float characterCriDamage;



    private void Awake()
    {
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
            //skill.Init();
            //skill.SetTarget();

        }
    }

    public void AddSkill(int id)
    {

    }
}