using UnityEngine;
using UnityEngine.UI;

public class ExtremColdCrackWindow : MonoBehaviour
{
    public CrackOpen crackOpen;
    public GameObject ExtremColdSelectStage;
    [Header("Buttons")]
    public Button ExtremColdCrackOpen;
    public Button BeforeCrackButton;
    public Button NextCrackButton;

    [SerializeField] private ButtonAudio ButtonAudio;

    private void Awake()
    {
        ExtremColdCrackOpen.onClick.AddListener(ExtremColdCrackOpenButtonClick);
        NextCrackButton.onClick.AddListener(NextCrackButtonClick);
        BeforeCrackButton.onClick.AddListener(BeforeCrackButtonClick);
        ExtremColdCrackOpen.onClick.AddListener(ButtonAudio.PlayClickSound);
        NextCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        BeforeCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void ExtremColdCrackOpenButtonClick()
    {
        ExtremColdSelectStage.SetActive(true);
    }

    private void BeforeCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 2;
        SaveLoadManager.Save();
    }

    private void NextCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 4;
        SaveLoadManager.Save();
    }

}
