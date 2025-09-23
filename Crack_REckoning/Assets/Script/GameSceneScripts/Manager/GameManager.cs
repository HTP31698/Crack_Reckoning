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
        SetSkillStat(0, 31001);

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
                stageManager.wave20Spawned = true; // ���Ͱ� ������ �� Ȯ��
            }

            if (stageManager.wave20Spawned && !MonsterManager.HasMonster())
            {
                ClearWindowPause(); // ���� �� ����� ���� Ŭ����
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

        // ���� ����
        if (!enforceLevel.ContainsKey(id)) enforceLevel[id] = 0;
        enforceLevel[id]++;

        // UI ����
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
        StageSkillList.Add(31001);
    }

    // using�� ���� �ִٰ� ����
    // using System.Linq;

    public void ShowLevelUpSkills()
    {
        // ���� ����/���� ���
        var equipped = character.GetSkillIdList(); // ���� �� ����(�ִ� 5)
        var data = SaveLoadManager.Data;

        // ��� �ڵ�
        if (data == null || data.OwnedSkillIds == null || equipped == null)
            return;

        // �ĺ� �и�
        List<int> owned = data.OwnedSkillIds;
        List<int> unlockables = new List<int>(); // ���� ���� ����
        List<int> upgradables = new List<int>(); // ���׷��̵�(�̹� ������)

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

        // �ִ� 3�� ����
        int want = 3;

        // 1) ���� ������ unlockables���� ���� �߰�
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
                    pendingPickIds.Add(0); // 0 = �� ����
                }
                unlockables.RemoveAt(idx);
                if (pendingSkillOptions.Count >= want) break;
            }
        }

        // 2) �������� ���׷��̵� �ĺ����� ä���
        while (pendingSkillOptions.Count < want && upgradables.Count > 0)
        {
            int idx = Random.Range(0, upgradables.Count);
            int id = upgradables[idx];
            if (!pendingSkillOptions.Contains(id))
            {
                // ���׷��̵�� pickId�� �̾Ƶд� (���̺� ������ ���� 1..5 ����)
                int pickId = Random.Range(1, 6);
                pendingSkillOptions.Add(id);
                pendingPickIds.Add(pickId);
            }
            upgradables.RemoveAt(idx);
        }

        // �ĺ��� �ϳ��� ������ ����
        if (pendingSkillOptions.Count == 0) return;

        // ��ư ����
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
                // ���׷��̵�� Selection �̸� ä���
                var ss = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable).Get(skillId, pickId);
                if (ss != null && !string.IsNullOrEmpty(ss.SkillPickName))
                    label = ss.SkillPickName;
            }
            // �� ����(pickId==0)�� ���� ��ų �⺻ �̸� ǥ��

            if (tmpText != null) tmpText.text = label;
            if (img != null && s != null) img.sprite = s.sprite;

            int index = i;
            Buttons[i].onClick.RemoveAllListeners();
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
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
            // ���� ���� (���� ���� ����)
            if (equipped.Count < 5 && !equipped.Contains(skillId))
            {
                character.AddSkill(skillId);
                // ��� �߰��� ��ų�� ����Ʈ ���� ���Ƿ� index = ���� count (�߰� ��)
                SetSkillStat(equipped.Count - 1, skillId);
            }
            else
            {
                // ������ ���ų� �̹� �����Ǿ� ������ ����
            }
        }
        else
        {
            // ���׷��̵� (�̹� �����Ǿ� �־�� ��)
            if (equipped.Contains(skillId))
            {
                var selectionTable = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable);
                var skillData = selectionTable.Get(skillId, pickId);
                if (skillData != null)
                {
                    character.IncreaseSkill(skillId, skillData); // �ΰ��� �ӽ� ������
                    UpdateSkillStat(skillId);                    // UI +n ����
                }
            }
        }

        // â �ݱ�
        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
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
        PlaySetting.SetSelectStage(PlaySetting.SelectStage + 1);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void ExitStageButtonClick()
    {
        // �� ��η� ���� �ε��� Ȯ�� (������Ʈ ���� ���)
        const string scenePath = "Assets/Scenes/LobbyScene.unity";
        int index = SceneUtility.GetBuildIndexByScenePath(scenePath);

        if (index < 0)
        {
            Debug.LogError($"Scene not in Build Settings: {scenePath}");
            return;
        }
        SceneManager.LoadScene(index);
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


