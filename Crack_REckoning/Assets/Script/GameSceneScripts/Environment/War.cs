using JetBrains.Annotations;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    public Slider warHpSlider;

    private int currentHp;
    private int maxHp;

    public void Awake()
    {
        maxHp = 3000;
        currentHp = maxHp;
    }

    protected void OnEnable()
    {
        warHpSlider.gameObject.SetActive(true);
        warHpSlider.value = (float)currentHp / (float)maxHp;
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        warHpSlider.value = (float)currentHp / (float)maxHp;
        if (currentHp < 0)
        {
            currentHp = 0;
            warHpSlider.value = (float)currentHp / (float)maxHp;
        }
    }
}