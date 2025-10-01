using UnityEngine;
using UnityEngine.UI;

public class GameSettingWindow : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private StageManager stageManager;

    [Header("Buttons")]
    public Button ExitButton;
    public Button CanCelButton;


    private void Awake()
    {
        ExitButton.onClick.RemoveAllListeners();
        CanCelButton.onClick.RemoveAllListeners();

        ExitButton.onClick.AddListener(FailedWindowPause);
        CanCelButton.onClick.AddListener(CanCelButtonClick);
    }


    private void FailedWindowPause()
    {
        for (int i = 0; i < 3; i++)
        {
            gameManager.Buttons[i].gameObject.SetActive(false);
        }
        stageManager.ShowFailedWindow();
        gameManager.PauseGame();
    }
    private void CanCelButtonClick()
    {
        gameManager.ResumeGame();
        gameObject.SetActive(false);
    }
}