using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject HomeWindow;
    public GameObject StageSelctWindow;

    public Button StageChallengeButton;
    public Button Stage1Button;

    public Button CharacterButton;
    public Button SkillButton;
    public Button HomeButton;
    public Button PetButton;
    public Button EnforceButton;

    public TextMeshProUGUI Gold;

    public int gold = 0;


    public void Awake()
    {
        HomeWindow.SetActive(true);
        StageSelctWindow.SetActive(false);

        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);
        Stage1Button.onClick.AddListener(Stage1ButtonClick);

        HomeButton.onClick.AddListener(HomeButtonClick);

    }


    public void StageChallengeButtonClick()
    {
        HomeWindow.SetActive(false);
        StageSelctWindow.SetActive(true);
    }

    public void HomeButtonClick()
    {
        HomeWindow.SetActive(true);
        StageSelctWindow.SetActive(false);
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