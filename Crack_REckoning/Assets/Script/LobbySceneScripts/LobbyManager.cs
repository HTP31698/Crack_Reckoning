using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject MainWindow;
    public GameObject StageSelctWindow;

    public Button StageChallengeButton;
    public Button Stage1Button;


    public void Awake()
    {
        MainWindow.SetActive(true);
        StageSelctWindow.SetActive(false);

        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);
        Stage1Button.onClick.AddListener(Stage1ButtonClick);
    }


    public void StageChallengeButtonClick()
    {
        MainWindow.SetActive(false);
        StageSelctWindow.SetActive(true);
    }


    public void Stage1ButtonClick()
    {
        PlaySetting.SelectStage = 1;

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