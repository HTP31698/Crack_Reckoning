using UnityEngine;
using UnityEngine.UI;

public class PigCrackWindow : MonoBehaviour
{
    public CrackOpen crackOpen;
    public GameObject PigSelectStage;
    [Header("Buttons")]
    public Button PigCrackOpen;
    public Button NextCrackButton;

    [SerializeField] private ButtonAudio ButtonAudio;

    private void Awake()
    {
        PigCrackOpen.onClick.AddListener(PigCrackOpenButtonClick);
        NextCrackButton.onClick.AddListener(NextCrackButtonClick);
        PigCrackOpen.onClick.AddListener(ButtonAudio.PlayClickSound);
        NextCrackButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void PigCrackOpenButtonClick()
    {
        PigSelectStage.SetActive(true);
    }

    private void NextCrackButtonClick()
    {
        gameObject.SetActive(false);
        crackOpen.gameObject.SetActive(true);
        var data = SaveLoadManager.Data;
        data.CurrentCrack = 2;
        SaveLoadManager.Save();
    }

}
