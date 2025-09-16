using UnityEngine;

public class Character : MonoBehaviour
{
    private static readonly string CharacterTable = "CharacterTable";

    public GameObject[] skills;

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

    public void Init()
    {

    }

    public void InitPlayerData()
    {

    }

    public void FireBall()
    {
        
    }
}