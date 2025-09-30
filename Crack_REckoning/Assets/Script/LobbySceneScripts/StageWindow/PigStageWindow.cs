using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PigStageWindow : MonoBehaviour
{
    private const string GameScene = "GameScene";

    [Header("Buttons")]
    public Button[] StageButton;
    public Button ExitButton;

    [SerializeField] private ButtonAudio buttonAudio;

    private int StageCount = 1;

    private void Awake()
    {
        var data = SaveLoadManager.Data;

        for (int i = 0; i <= 4; i++)
        {
            StageButton[i].interactable = false;
        }

        SetupUI();
    }

    private void SetupUI()
    {
        var data = SaveLoadManager.Data;
        for (int i = 0; i < data.StageClear.Count; i++)
        {
            if (GetStageClear(i)) StageCount++;
        }

        if (StageCount > StageButton.Length) StageCount = StageButton.Length;
        if (StageCount < 1) StageCount = 1;

        for (int i = 0; i < StageCount && i < StageButton.Length; i++)
        {
            StageButton[i].onClick.RemoveAllListeners();
            int idx = i;
            StageButton[i].interactable = true;
            StageButton[i].onClick.AddListener(() => StageButtonClick(idx));
            StageButton[i].onClick.AddListener(buttonAudio.PlayClickSound);
        }

        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(buttonAudio.PlayClickSound);
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
        PlaySetting.SetSelectStage(stage + 1);

        SceneManager.LoadScene(GameScene);
    }

    private void ExitButtonClick()
    {
        gameObject.SetActive(false);
    }
}
