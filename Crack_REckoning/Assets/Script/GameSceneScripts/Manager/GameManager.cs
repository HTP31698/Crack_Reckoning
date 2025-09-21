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

    public Character character;
    public Button[] Buttons;
    public Toggle toggle;
    private List<int> StageSkillList;

    private List<int> pendingSkillOptions;
    private List<int> pendingPickIds;

    public GameObject SettingsWindow;
    public Button SettingButton;
    public Button TryagainButton;
    public Button GiveupButton;

    public TextMeshProUGUI timerText;
    private float timer = 0f;

    private float TimeSet = 1f;
    bool isStop = false;

    public Slider[] sliderSkills;
    private Dictionary<int, int> skillIndex;
    private Dictionary<int, int> enforceLevel;
    private float[] coolTimer;
    private float[] skillCool;

    private void Awake()
    {
        StageSkillList = new List<int>();
        skillIndex = new Dictionary<int, int>();
        enforceLevel = new Dictionary<int, int>();
        coolTimer = new float[sliderSkills.Length];
        skillCool = new float[sliderSkills.Length];

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
        GiveupButton.onClick.AddListener(GiveupButtonClick);
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

        if (coolTimer != null && skillCool != null && sliderSkills != null && !character.isUseSkill)
        {
            int n = Mathf.Min(sliderSkills.Length, coolTimer.Length, skillCool.Length);

            float delta = Time.deltaTime;

            for (int i = 0; i < n; i++)
            {
                var slider = sliderSkills[i];
                if (slider == null || !slider.gameObject.activeInHierarchy)
                    continue;

                float total = skillCool[i];
                if (total <= 0f)
                {
                    coolTimer[i] = 0f;
                    slider.value = 1f;
                    continue;
                }
                coolTimer[i] += delta;

                if (coolTimer[i] >= total)
                {
                    coolTimer[i] = 0f;
                }
                slider.value = Mathf.Clamp01(coolTimer[i] / total);
            }
        }
    }

   private void SetSkillStat(int index, int id)
    {
        SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(id);
        sliderSkills[index].gameObject.SetActive(true);
        var bg = sliderSkills[index].transform.Find("Background").GetComponent<Image>();
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

        // UI 텍스트 반영
        var text = sliderSkills[index].transform.Find("Enforce")?.GetComponent<TextMeshProUGUI>();
        if (text != null) text.text = $"+{enforceLevel[id]}";

        // 쿨타임 값 세팅 (슬롯 기준으로 관리)
        coolTimer[index] = 0f;
        skillCool[index] = s.SkillCoolTime;

        sliderSkills[index].value = (skillCool[index] > 0f) ? coolTimer[index] / skillCool[index] : 0f;

    }

    private void UpdateSkillStat(int id)
    {
        SkillData s = DataTableManager.Get<SkillTable>(SkillTable).Get(id);
        skillCool[skillIndex[id]] = s.SkillCoolTime;

        sliderSkills[skillIndex[id]].value = (skillCool[skillIndex[id]] > 0f) ? 
            coolTimer[skillIndex[id]] / skillCool[skillIndex[id]] : 0f;

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
    private void GiveupButtonClick()
    {
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


