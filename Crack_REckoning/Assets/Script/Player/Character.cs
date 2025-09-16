using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

    public GameObject[] skills = new GameObject[5];
    private int skillCount = 1;

    private CharacterDataTable characterDataTable;
    private CharacterData characterData;
    //skillSelectionDataTable

    private int id;
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
        if(Input.GetKeyDown(KeyCode.R))
        {
            UseSkill();
        }
    }

    public void UseSkill()
    {
        Monster target = MonsterManager.nearMonster(gameObject.transform.position);
        if(target != null)
        {
            target.TakeDamage(100);
            Debug.Log($"Ω∫≈≥¿Ã{target.name}");
        }
    }

    public void AddSkill()
    { 
    }
}