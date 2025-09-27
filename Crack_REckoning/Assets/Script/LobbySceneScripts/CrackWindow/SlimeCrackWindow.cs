using UnityEngine;
using UnityEngine.UI;

public class SlimeCrackWindow : MonoBehaviour
{
    public CrackOpen crackOpen;
    public GameObject PigSelectStage;
    [Header("Buttons")]
    public Button SlimeCrackOpen;
    public Button BeforeCrackButton;
    public Button NextCrackButton;

    private void Awake()
    {
        SlimeCrackOpen.onClick.AddListener(SlimeCrackOpenButtonClick);
        NextCrackButton.onClick.AddListener(NextCrackButtonClick);
        BeforeCrackButton.onClick.AddListener(BeforeCrackButtonClick);
    }

    private void SlimeCrackOpenButtonClick()
    {
        PigSelectStage.SetActive(true);
    }

    private void BeforeCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 1;
        SaveLoadManager.Save();
    }

    private void NextCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 3;
        SaveLoadManager.Save();
    }

}
