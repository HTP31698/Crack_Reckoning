using UnityEngine;
using UnityEngine.UI;

public class SettingWindow : MonoBehaviour
{
    public GameObject NewIdWindow;
    [SerializeField] private ButtonAudio ButtonAudio;


    [Header("Buttons")]
    public Button ExitButton;
    public Button NewIdWindowOpenButton;
    public Button NewIdButton;
    public Button NewIdCancelButton;


    private void Awake()
    {
        ExitButton.onClick.AddListener(ExitButtonClick);
        NewIdWindowOpenButton.onClick.AddListener(NewIdWindowOpenButtonClick);
        NewIdButton.onClick.AddListener(NewIdButtonClick);
        NewIdCancelButton.onClick.AddListener(NewIdCancelButtonClick);

        ExitButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        NewIdWindowOpenButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        NewIdButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        NewIdCancelButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void ExitButtonClick()
    {
        gameObject.SetActive(false);
    }
    private void NewIdWindowOpenButtonClick()
    {
        NewIdWindow.SetActive(true);
    }
    private void NewIdButtonClick()
    {
        SaveLoadManager.Data.OwnedSkillIds.Clear();
        SaveLoadManager.Data.EquipmentSkillIds.Clear();
        SaveLoadManager.Data.OwnedCharacterIds.Clear();
        SaveLoadManager.Data.OwnedPetIds.Clear();
        SaveLoadManager.Data.GetSkills.Clear();
        SaveLoadManager.Data.GetPets.Clear();
        SaveLoadManager.Data.StageClear.Clear();

        SaveLoadManager.Data.OwnedCharacterIds.Add(1001);

        SaveLoadManager.Data.OwnedSkillIds.Add(3007);
        SaveLoadManager.Data.OwnedSkillIds.Add(3002);
        SaveLoadManager.Data.OwnedSkillIds.Add(3003);
        SaveLoadManager.Data.OwnedSkillIds.Add(3001);
        SaveLoadManager.Data.OwnedSkillIds.Add(3010);

        SaveLoadManager.Data.OwnedPetIds.Add(7001);
        SaveLoadManager.Data.OwnedPetIds.Add(7002);
        SaveLoadManager.Data.OwnedPetIds.Add(7003);
        SaveLoadManager.Data.OwnedPetIds.Add(7004);

        SaveLoadManager.Data.EquipmentSkillIds.Add(3007);
        SaveLoadManager.Data.EquipmentPetId = 7001;
        SaveLoadManager.Data.PlayerID = 1001;
        SaveLoadManager.Data.Gold = 0;
        SaveLoadManager.Data.CurrentCrack = 1;

        for (int i = 0; i < 25; i++)
        {
            SaveLoadManager.Data.StageClear[i] = false;
        }
        for (int i = 0; i < 10; i++)
        {
            SaveLoadManager.Data.GetSkills[i] = false;
        }
        for (int i = 0; i < 4; i++)
        {
            SaveLoadManager.Data.GetPets[i] = false;
        }

        SaveLoadManager.Save(0);
        Application.Quit();
    }
    private void NewIdCancelButtonClick()
    {
        NewIdWindow.SetActive(false);
    }
}
