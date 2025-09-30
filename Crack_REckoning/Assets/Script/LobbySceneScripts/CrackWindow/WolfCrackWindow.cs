using UnityEngine;
using UnityEngine.UI;

public class WolfCrackWindow : MonoBehaviour
{
    public CrackOpen crackOpen;
    public GameObject WolfSelectStage;
    [Header("Buttons")]
    public Button WolfCrackOpen;
    public Button BeforeCrackButton;
    public Button NextCrackButton;

    [SerializeField] private ButtonAudio ButtonAudio;


    private void Awake()
    {
        WolfCrackOpen.onClick.AddListener(SlimeCrackOpenButtonClick);
        NextCrackButton.onClick.AddListener(NextCrackButtonClick);
        BeforeCrackButton.onClick.AddListener(BeforeCrackButtonClick);
        WolfCrackOpen.onClick.AddListener(ButtonAudio.PlayClickSound);
        NextCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        BeforeCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void SlimeCrackOpenButtonClick()
    {
        WolfSelectStage.SetActive(true);
    }

    private void BeforeCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 3;
        SaveLoadManager.Save();
    }

    private void NextCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 5;
        SaveLoadManager.Save();
    }
}
