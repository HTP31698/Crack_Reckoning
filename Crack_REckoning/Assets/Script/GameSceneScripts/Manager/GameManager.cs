using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static readonly string SkillSelectionTable = "SkillSelectionTable";
    private static readonly string SkillTable = "SkillTable";
    private static readonly string Icon = "Icon";
    private static readonly string Background = "Background";
    private static readonly string Enforce = "Enforce";

    public Character character;
    public StageManager stageManager;

    public Button[] Buttons;
    public Toggle toggle;
    private List<int> StageSkillList;

    private List<int> pendingSkillOptions;
    private List<int> pendingPickIds;

    public GameObject SettingsWindow;
    public Button SettingButton;
    public Button TryagainButton;
    public Button GiveupButton;

    public Button NextStageButton;
    public Button RetryStageButton;
    public Button ExitStageButton;

    public TextMeshProUGUI timerText;
    private float timer = 0f;

    private float TimeSet = 1f;
    bool isStop = false;

    public Slider[] sliderSkills;
    private Dictionary<int, int> skillIndex;
    private Dictionary<int, int> enforceLevel;

    private void Awake()
    {
        StageSkillList = new List<int>();
        skillIndex = new Dictionary<int, int>();
        enforceLevel = new Dictionary<int, int>();

        StageSkillListInit();

        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            int index = i;
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
        }
        for (int i = 1; i < sliderSkills.Length; i++)
        {
            sliderSkills[i].gameObject.SetActive(false);
        }
        SetSkillStat(0, PlaySetting.PlayerBasicSkillID);

        SettingsWindow.gameObject.SetActive(false);

        SettingButton.onClick.AddListener(OnSetButtonClick);
        TryagainButton.onClick.AddListener(OffsetButtonClick);
        GiveupButton.onClick.AddListener(FailedWindowPause);

        NextStageButton.onClick.AddListener(NextStageButtonClick);
        RetryStageButton.onClick.AddListener(RetryButtonClick);
        ExitStageButton.onClick.AddListener(ExitStageButtonClick);
    }

    public void Update()
    {
        if (!toggle.isOn && !isStop)
        {
            TimeSet = 1f;
            Time.timeScale = TimeSet;
        }
        else if (toggle.isOn && !isStop)
        {
            TimeSet = 2f;
            Time.timeScale = TimeSet;
        }

        timer += Time.deltaTime;
        int minutes = (int)(timer / 60);
        int seconds = (int)(timer % 60);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        if (stageManager.currentWave == 20)
        {
            if (!stageManager.wave20Spawned && MonsterManager.HasMonster())
            {
                stageManager.wave20Spawned = true; // 몬스터가 생성된 걸 확인
            }

            if (stageManager.wave20Spawned && !MonsterManager.HasMonster())
            {
                ClearWindowPause(); // 몬스터 다 잡았을 때만 클리어
            }
        }
    }

   private void SetSkillStat(int index, int id)
    {
        SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(id);
        sliderSkills[index].gameObject.SetActive(true);
        var bg = sliderSkills[index].transform.Find(Background).GetComponent<Image>();
        if (bg != null)
        {
            bg.sprite = s.sprite;
        }
        var fill = sliderSkills[index].fillRect.GetComponent<Image>();
        if (fill != null)
        {
            fill.sprite = s.sprite;
        }
        skillIndex[id] = index;
        if (!enforceLevel.ContainsKey(id)) enforceLevel[id] = 0;

        var text = sliderSkills[index].transform.Find(Enforce)?.GetComponent<TextMeshProUGUI>();
        if (text != null) text.text = $"+{enforceLevel[id]}";

    }

    private void UpdateSkillStat(int id)
    {
        SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(id);

        if (!skillIndex.TryGetValue(id, out int index))
            return;

        // 레벨 증가
        if (!enforceLevel.ContainsKey(id)) enforceLevel[id] = 0;
        enforceLevel[id]++;

        // UI 갱신
        var text = sliderSkills[index].transform.Find("Enforce")?.GetComponent<TextMeshProUGUI>();
        if (text != null) text.text = $"+{enforceLevel[id]}";
    }

    private void StageSkillListInit()
    {
        var skillTable = DataTableManager.Get<SkillTable>(SkillTable);
        StageSkillList.Clear();
        foreach (var id in skillTable.GetIdList())
        {
            if (skillTable.Get(id).SkillSortation == SkillSortationID.StageSkill)
                StageSkillList.Add(id);
        }
        StageSkillList.Add(character.BasicSkill);
    }

    public void ShowLevelUpSkills()
    {
        var characterSkillList = character.GetSkillIdList();
        pendingSkillOptions = new List<int>();
        pendingPickIds = new List<int>();

        while (pendingSkillOptions.Count < 3)
        {
            int randSkill = StageSkillList[Random.Range(0, StageSkillList.Count)];

            if (characterSkillList.Contains(randSkill))
            {
                if (!pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
            else
            {
                if (characterSkillList.Count < 5 && !pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(true);

            TMP_Text tmpText = Buttons[i].GetComponentInChildren<TMP_Text>();
            Image img = Buttons[i].transform.Find(Icon).GetComponent<Image>();

            int skillId = pendingSkillOptions[i];
            SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(skillId);

            int pickId;
            SkillSelectionData ss = null;

            if (characterSkillList.Contains(skillId))
            {
                pickId = Random.Range(1, 6);
                ss = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable).Get(skillId, pickId);
            }
            else
            {
                pickId = 0;
            }

            pendingPickIds.Add(pickId);

            if (tmpText != null)
                tmpText.text = (ss != null) ? ss.SkillPickName : s.SkillName;
            if (img != null)
                img.sprite = s.sprite;

            int index = i;
            Buttons[i].onClick.RemoveAllListeners();
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
        }

        PauseGame();
    }

    private void OnSkillButtonClick(int index)
    {
        if (index >= pendingSkillOptions.Count)
            return;

        int skillId = pendingSkillOptions[index];
        int pickId = (pendingPickIds != null && index < pendingPickIds.Count) ? pendingPickIds[index] : 0;

        var characterSkillList = character.GetSkillIdList();
        var selectionTable = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable);

        if (characterSkillList.Contains(skillId))
        {
            if (pickId > 0)
            {
                SkillSelectionData skillData = selectionTable.Get(skillId, pickId);
                if (skillData != null)
                {
                    character.IncreaseSkill(skillId, skillData);
                    UpdateSkillStat(skillId);
                }
            }
        }
        else
        {
            if (characterSkillList.Count < 5)
            {
                character.AddSkill(skillId);
                SetSkillStat(characterSkillList.Count, skillId);
            }
        }

        foreach (var btn in Buttons)
            btn.gameObject.SetActive(false);
        ResumeGame();
    }

    private void OnSetButtonClick()
    {
        SettingsWindow.gameObject.SetActive(true);
        PauseGame();
    }
    private void OffsetButtonClick()
    {
        SettingsWindow.gameObject.SetActive(false);
        ResumeGame();
    }
    private void ClearWindowPause()
    {
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
        stageManager.ShowClearWindow();
        PauseGame();
    }
    private void FailedWindowPause()
    {
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
        stageManager.ShowFailedWindow();
        PauseGame();
    }

    private void RetryButtonClick()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void NextStageButtonClick()
    {
        PlaySetting.SelectStage++;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void ExitStageButtonClick()
    {
        PlaySetting.SelectStage--;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void PauseGame()
    {
        isStop = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isStop = false;
        Time.timeScale = TimeSet;
    }
}


