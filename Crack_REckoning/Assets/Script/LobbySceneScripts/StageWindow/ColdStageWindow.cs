using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ColdStageWindow : MonoBehaviour
{
    private const string GameScene = "GameScene";

    [Header("Buttons")]
    public Button[] StageButton;
    public Button ExitButton;

    private int StageCount = 0;

    private void Awake()
    {
        var data = SaveLoadManager.Data;

        for (int i = 0; i < StageButton.Length; i++)
        {
            StageButton[i].interactable = false;
        }

        SetupUI();
    }

    private void SetupUI()
    {
        var data = SaveLoadManager.Data;
        for (int i = 9; i < 14; i++)
        {
            if (GetStageClear(i)) StageCount++;
        }

        if (StageCount > StageButton.Length) StageCount = StageButton.Length;

        for (int i = 0; i < StageCount && i < StageButton.Length; i++)
        {
            StageButton[i].onClick.RemoveAllListeners();
            int idx = i;
            StageButton[i].interactable = true;
            StageButton[i].onClick.AddListener(() => StageButtonClick(idx));
        }

        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(ExitButtonClick);
    }

    private bool GetStageClear(int index)
    {
        bool v;
        if (SaveLoadManager.Data.StageClear.TryGetValue(index, out v))
            return v;
        return false;
    }

    private void StageButtonClick(int stage)
    {
        PlaySetting.SetSelectStage(stage + 11);

        SceneManager.LoadScene(GameScene);
    }

    private void ExitButtonClick()
    {
        gameObject.SetActive(false);
    }
}
