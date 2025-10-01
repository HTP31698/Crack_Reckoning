using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Windows")]
    public GameObject HomeWindow;
    public GameObject SkillWindow;
    public GameObject SkillEnforceWindow;
    public GameObject CrackOpenWindow;
    public GameObject ChWindow;
    public GameObject ChEnforceWindow;
    public GameObject PetWindow;
    public GameObject PetEnforceWindow;
    public GameObject SettingWindow;
    public GameObject NewIdWindow;

    [Header("CrackWindows")]
    public GameObject PigCrackWindow;
    public GameObject SlimeCrackWindow;
    public GameObject ExtremColdCrackWindow;
    public GameObject WolfCrackWindow;
    public GameObject MarionetteCrackWindow;

    [Header("StageWindows")]
    public GameObject PigStageWindow;
    public GameObject SlimeStageWindow;
    public GameObject ExtremColdStageWindow;
    public GameObject WolfStageWindow;
    public GameObject MarionetteStageWindow;

    [Header("Buttons")]
    public Button StageChallengeButton;
    public Button CharacterButton;
    public Button SkillButton;
    public Button HomeButton;
    public Button PetButton;
    public Button EnforceButton;
    public Button MenuButton;

    [Header("Texts")]
    public TextMeshProUGUI Gold;

    [SerializeField] private ButtonAudio ButtonAudio;



    public void Awake()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();


        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);
        HomeButton.onClick.AddListener(HomeButtonClick);
        SkillButton.onClick.AddListener(SkillButtonClick);
        CharacterButton.onClick.AddListener(CharacterButtonClick);
        PetButton.onClick.AddListener(PetButtonClick);
        MenuButton.onClick.AddListener(MenuButtonClick);

        StageChallengeButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        HomeButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        SkillButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        CharacterButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        PetButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        MenuButton.onClick.AddListener(ButtonAudio.PlayClickSound);

        Gold.text = SaveLoadManager.Data.Gold.ToString();
    }

    public void StageChallengeButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(true);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void HomeButtonClick()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void SkillButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(true);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void CharacterButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(true);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void PetButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(true);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void MenuButtonClick()
    {
        SettingWindow.SetActive(true);
    }



    private void CrackWindowSetFalse()
    {
        PigCrackWindow.SetActive(false);
        SlimeCrackWindow.SetActive(false);
        ExtremColdCrackWindow.SetActive(false);
        WolfCrackWindow.SetActive(false);
        MarionetteCrackWindow.SetActive(false);
    }

    private void StageWindowSetFalse()
    {
        PigStageWindow.SetActive(false);
        SlimeStageWindow.SetActive(false);
        ExtremColdStageWindow.SetActive(false);
        WolfStageWindow.SetActive(false);
        MarionetteStageWindow.SetActive(false);
    }
}