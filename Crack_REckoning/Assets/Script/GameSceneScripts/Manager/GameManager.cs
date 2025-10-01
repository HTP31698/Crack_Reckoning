using System.Collections.Generic;
using System.Text;
using TMPro;
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

    [SerializeField] private Character character;
    [SerializeField] private StageManager stageManager;

    [Header("SettingWindows")]
    public GameObject PigSet;
    public GameObject SlimeSet;
    public GameObject ColdSet;
    public GameObject WolfSet;
    public GameObject DollSet;



    public Button[] Buttons;
    public Toggle toggle;
    private List<int> StageSkillList;

    private List<int> pendingSkillOptions;
    private List<int> pendingPickIds;

    public Button SettingButton;

    public Button NextStageButton;
    public Button RetryStageButton;
    public Button ExitStageButton;

    public TextMeshProUGUI timerText;
    private float timer = 0f;

    private float TimeSet = 1f;
    public bool isStop = false;
    private bool isClear = false;

    public Slider[] sliderSkills;
    private Dictionary<int, int> skillIndex;
    private Dictionary<int, int> enforceLevel;

    [SerializeField] private ButtonAudio ButtonAudio;

    private void Awake()
    {
        StageSkillList = new List<int>();
        skillIndex = new Dictionary<int, int>();
        enforceLevel = new Dictionary<int, int>();

       

        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            int index = i;
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
            Buttons[i].onClick.AddListener(ButtonAudio.PlayClickSound);
        }
        for (int i = 1; i < sliderSkills.Length; i++)
        {
            sliderSkills[i].gameObject.SetActive(false);
        }

        PigSet.gameObject.SetActive(false);
        SlimeSet.gameObject.SetActive(false);
        ColdSet.gameObject.SetActive(false);
        WolfSet.gameObject.SetActive(false);
        DollSet.gameObject.SetActive(false);

        SettingButton.onClick.AddListener(OnSetButtonClick);

        NextStageButton.onClick.AddListener(NextStageButtonClick);
        RetryStageButton.onClick.AddListener(RetryButtonClick);
        ExitStageButton.onClick.AddListener(ExitStageButtonClick);

        SettingButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        NextStageButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        RetryStageButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        ExitStageButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void Start()
    {
        StageSkillListInit();
        var data = SaveLoadManager.Data;
        if(data?.EquipmentSkillIds != null && data.EquipmentSkillIds.Count > 0)
        {
            SetSkillStat(0, SaveLoadManager.Data.EquipmentSkillIds[0]);
        }
        isClear = false;
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
                stageManager.wave20Spawned = true;
            }

            if (stageManager.wave20Spawned && !MonsterManager.HasMonster() && !isClear)
            {
                ClearWindowPause();
                return;
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
            StageSkillList.Add(id);
        }
        StageSkillList.Add(3001);
    }

    public void ShowLevelUpSkills()
    {
        var equipped = character.GetSkillIdList();
        var data = SaveLoadManager.Data;

        if (data == null || data.OwnedSkillIds == null || equipped == null)
            return;

        List<int> owned = data.OwnedSkillIds;
        List<int> unlockables = new List<int>();
        List<int> upgradables = new List<int>();

        foreach (var id in owned)
        {
            if (id <= 0) continue;
            if (equipped.Contains(id)) upgradables.Add(id);
            else unlockables.Add(id);
        }

        pendingSkillOptions = new List<int>();
        pendingPickIds = new List<int>();

        int maxSlots = 5;
        bool hasEmptySlot = equipped.Count < maxSlots;

        int want = 3;

        if (hasEmptySlot && unlockables.Count > 0)
        {
            int take = Mathf.Min(want - pendingSkillOptions.Count, unlockables.Count);
            for (int k = 0; k < take; k++)
            {
                int idx = Random.Range(0, unlockables.Count);
                int id = unlockables[idx];
                if (!pendingSkillOptions.Contains(id))
                {
                    pendingSkillOptions.Add(id);
                    pendingPickIds.Add(0);
                }
                unlockables.RemoveAt(idx);
                if (pendingSkillOptions.Count >= want) break;
            }
        }

        while (pendingSkillOptions.Count < want && upgradables.Count > 0)
        {
            int idx = Random.Range(0, upgradables.Count);
            int id = upgradables[idx];
            if (!pendingSkillOptions.Contains(id))
            {
                int pickId = Random.Range(1, 6);
                pendingSkillOptions.Add(id);
                pendingPickIds.Add(pickId);
            }
            upgradables.RemoveAt(idx);
        }

        if (pendingSkillOptions.Count == 0) return;

        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].gameObject.SetActive(false);

        for (int i = 0; i < pendingSkillOptions.Count && i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(true);

            TMP_Text tmpText = Buttons[i].GetComponentInChildren<TMP_Text>();
            Image img = Buttons[i].transform.Find(Icon).GetComponent<Image>();

            int skillId = pendingSkillOptions[i];
            int pickId = pendingPickIds[i];

            SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(skillId);
            string label = s != null ? s.SkillName : $"Skill {skillId}";

            if (pickId > 0)
            {
                var ss = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable).Get(skillId, pickId);
                if (ss != null && !string.IsNullOrEmpty(ss.SkillPickName))
                    label = ss.SkillPickName;
            }

            if (tmpText != null) tmpText.text = label;
            if (img != null && s != null) img.sprite = s.sprite;

            int index = i;
            Buttons[i].onClick.RemoveAllListeners();
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
            Buttons[i].onClick.AddListener(ButtonAudio.PlayClickSound);
        }

        PauseGame();
    }

    private void OnSkillButtonClick(int index)
    {
        if (index < 0 || index >= pendingSkillOptions.Count) return;

        int skillId = pendingSkillOptions[index];
        int pickId = (pendingPickIds != null && index < pendingPickIds.Count) ? pendingPickIds[index] : 0;

        var equipped = character.GetSkillIdList();
        if (equipped == null) return;

        if (pickId == 0)
        {
            if (equipped.Count < 5 && !equipped.Contains(skillId))
            {
                character.AddSkill(skillId);
                SetSkillStat(equipped.Count, skillId);
            }
            else
            {
            }
        }
        else
        {
            if (equipped.Contains(skillId))
            {
                var selectionTable = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable);
                var skillData = selectionTable.Get(skillId, pickId);
                if (skillData != null)
                {
                    character.IncreaseSkill(skillId, skillData);
                    UpdateSkillStat(skillId);
                }
            }
        }

        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
        ResumeGame();
    }


    private void OnSetButtonClick()
    {
        if (stageManager.currentStage <= 5)
        {
            PigSet.gameObject.SetActive(true);
        }
        else if (stageManager.currentStage <= 10)
        {
            SlimeSet.gameObject.SetActive(true);
        }
        else if (stageManager.currentStage <= 15)
        {
            ColdSet.SetActive(true);
        }
        else if (stageManager.currentStage <= 20)
        {
            WolfSet.SetActive(true);
        }
        else if (stageManager.currentStage <= 25)
        {
            DollSet.SetActive(true);
        }
        PauseGame();
    }

    private void ClearWindowPause()
    {
        if (isClear) return;
        isClear = true;
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
        stageManager.ShowClearWindow();
        PauseGame();
    }


    private void RetryButtonClick()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void NextStageButtonClick()
    {
        Time.timeScale = 1f;
        PlaySetting.SetSelectStage(PlaySetting.SelectStage + 1);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void ExitStageButtonClick()
    {
        Time.timeScale = 1f;
        const string scenePath = "Assets/Scenes/LobbyScene.unity";
        int index = SceneUtility.GetBuildIndexByScenePath(scenePath);

        if (index < 0)
        {
            Debug.LogError($"Scene not in Build Settings: {scenePath}");
            return;
        }
        SceneManager.LoadScene(index);
    }
    private bool IsLevelUpUIOpen()
    {
        for (int i = 0; i < 3 && i < Buttons.Length; i++)
            if (Buttons[i].gameObject.activeSelf)
                return true;
        return false;
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


