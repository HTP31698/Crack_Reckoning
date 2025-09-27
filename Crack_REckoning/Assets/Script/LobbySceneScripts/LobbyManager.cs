using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Windows")]
    public GameObject HomeWindow;
    public GameObject SkillWindow;
    public GameObject SkillEnforceWindow;
    public GameObject CrackOpenWindow;



    [Header("CrackWindows")]
    public GameObject PigCrackWindow;
    public GameObject SlimeCrackWindow;
    public GameObject ExtremColdCrackWindow;
    public GameObject WolfCrackWindow;
    public GameObject MarionetteCrackWindow;

    [Header("Buttons")]
    public Button StageChallengeButton;
    public Button CharacterButton;
    public Button SkillButton;
    public Button HomeButton;
    public Button PetButton;
    public Button EnforceButton;

    [Header("Texts")]
    public TextMeshProUGUI Gold;


    public void Awake()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        CrackWindowSetFalse();


        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);


        HomeButton.onClick.AddListener(HomeButtonClick);
        SkillButton.onClick.AddListener(SkillButtonClick);

        Gold.text = SaveLoadManager.Data.Gold.ToString();
    }


    public void StageChallengeButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(true);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        CrackWindowSetFalse();
    }

    public void HomeButtonClick()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        CrackWindowSetFalse();
    }

    public void SkillButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(true);
        SkillEnforceWindow.SetActive(false);
        CrackWindowSetFalse();
    }



    private void CrackWindowSetFalse()
    {
        PigCrackWindow.SetActive(false);
        SlimeCrackWindow.SetActive(false);
        ExtremColdCrackWindow.SetActive(false);
        WolfCrackWindow.SetActive(false);
        MarionetteCrackWindow.SetActive(false);
    }
}