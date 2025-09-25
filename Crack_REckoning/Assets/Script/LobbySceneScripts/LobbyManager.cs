using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject HomeWindow;
    public GameObject StageSelctWindow;
    public GameObject SkillWindow;
    public GameObject SkillEnforceWindow;

    public Button StageChallengeButton;
    public Button Stage1Button;

    public Button CharacterButton;
    public Button SkillButton;
    public Button HomeButton;
    public Button PetButton;
    public Button EnforceButton;

    public TextMeshProUGUI Gold;


    public void Awake()
    {
        HomeWindow.SetActive(true);
        StageSelctWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);

        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);
        Stage1Button.onClick.AddListener(Stage1ButtonClick);


        HomeButton.onClick.AddListener(HomeButtonClick);
        SkillButton.onClick.AddListener(SkillButtonClick);

        Gold.text = SaveLoadManager.Data.Gold.ToString();
    }


    public void StageChallengeButtonClick()
    {
        HomeWindow.SetActive(false);
        StageSelctWindow.SetActive(true);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
    }

    public void HomeButtonClick()
    {
        HomeWindow.SetActive(true);
        StageSelctWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
    }

    public void SkillButtonClick()
    {
        HomeWindow.SetActive(false);
        StageSelctWindow.SetActive(false);
        SkillWindow.SetActive(true);
        SkillEnforceWindow.SetActive(false);
    }



    public void Stage1ButtonClick()
    {
        PlaySetting.SetSelectStage(1);

        // 씬 경로로 빌드 인덱스 확인 (프로젝트 실제 경로)
        const string scenePath = "Assets/Scenes/GameScene.unity";
        int index = SceneUtility.GetBuildIndexByScenePath(scenePath);

        if (index < 0)
        {
            Debug.LogError($"Scene not in Build Settings: {scenePath}");
            return;
        }
        SceneManager.LoadScene(index);
    }


}