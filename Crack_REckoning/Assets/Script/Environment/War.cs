using JetBrains.Annotations;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    public TextMeshProUGUI hptext;
    public Monster monster;

    private int currentHp;
    private int maxHp;
    private float monsterLastAttack = 0;

    public void Start()
    {
        maxHp = 3000;
        currentHp = maxHp;
        hptext.text = currentHp.ToString();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            monster = other.gameObject.GetComponent<Monster>();
            if(monsterLastAttack > monster.attackSpeed)
            {

            }
            TakeDamage(monster.damage);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp < 0)
        {
            currentHp = 0;
        }
        hptext.text = currentHp.ToString();
    }
}