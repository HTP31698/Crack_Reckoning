using UnityEngine;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    public Slider warHpSlider;
    public StageManager stageManager;
    [SerializeField] private Pet pet;

    private int currentHp;
    private int maxHp;
    private bool isDead;

    private void Awake()
    {
        maxHp = 3000 + pet.GetWallHpUp();
        currentHp = maxHp;
        isDead = false;
    }

    private void OnEnable()
    {
        if (warHpSlider != null)
        {
            warHpSlider.gameObject.SetActive(true);
            warHpSlider.minValue = 0f;
            warHpSlider.maxValue = 1f;
            warHpSlider.value = (float)currentHp / maxHp;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, amount));
        if (warHpSlider != null)
            warHpSlider.value = (float)currentHp / maxHp;

        if (currentHp <= 0)
        {
            isDead = true;
            if (stageManager != null)
                stageManager.ShowFailedWindow();
            else
                Debug.LogWarning("StageManager reference is missing in War.");
        }
    }
}
