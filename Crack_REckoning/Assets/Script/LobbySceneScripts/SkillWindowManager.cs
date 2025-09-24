using UnityEngine;
using UnityEngine.UI;

public class SkillWindowManager : MonoBehaviour
{
    public Button[] SkillSelectButtons;


    private void Start()
    {
        SetButtons();
    }



    private void SetButtons()
    {
        for (int i = 0; i < SkillSelectButtons.Length; i++)
        {
            var stable = DataTableManager.Get<SkillTable>("SkillTable");
            var sdata = stable.Get(SaveLoadManager.Data.OwnedSkillIds[i]);
            SkillSelectButtons[i].image.sprite = sdata.sprite;
        }
    }
}