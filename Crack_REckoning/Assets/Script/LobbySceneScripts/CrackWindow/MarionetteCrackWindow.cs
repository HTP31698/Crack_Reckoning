using UnityEngine;
using UnityEngine.UI;

public class MarionetteCrackWindow : MonoBehaviour
{
    public CrackOpen crackOpen;
    public GameObject MarionetteSelectStage;
    [Header("Buttons")]
    public Button MarionetteCrackOpen;
    public Button BeforeCrackButton;
    public Button NextCrackButton;

    [SerializeField] private ButtonAudio ButtonAudio;

    private void Awake()
    {
        MarionetteCrackOpen.onClick.AddListener(MarionetteCrackOpenButtonClick);
        NextCrackButton.onClick.AddListener(NextCrackButtonClick);
        BeforeCrackButton.onClick.AddListener(BeforeCrackButtonClick);
        MarionetteCrackOpen.onClick.AddListener(ButtonAudio.PlayClickSound);
        NextCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        BeforeCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void MarionetteCrackOpenButtonClick()
    {
        MarionetteSelectStage.SetActive(true);
    }

    private void BeforeCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 4;
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
