using JetBrains.Annotations;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    public TextMeshProUGUI hptext;
    private Monster monster;

    private int currentHp;
    private int maxHp;

    public void Start()
    {
        maxHp = 3000;
        currentHp = maxHp;
        hptext.text = currentHp.ToString();
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