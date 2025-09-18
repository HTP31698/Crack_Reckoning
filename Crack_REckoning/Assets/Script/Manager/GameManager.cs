using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Character character;
    public Button[] Buttons;
    public Toggle toggle;
    private List<int> StageSkillList;
    private List<int> pendingSkillOptions;

    public GameObject SettingsWindow;
    public Button SettingButton;
    public Button TryagainButton;
    public Button GiveupButton;

    private float TimeSet = 1f;
    bool isStop = false;


    private void Awake()
    {
        StageSkillList = new List<int>();
        StageSkillListInit();
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            int index = i;
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
        }
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
    }

    private void StageSkillListInit()
    {
        var skillTable = DataTableManager.Get<SkillTable>("SkillTable");
        StageSkillList.Clear();
        foreach (var id in skillTable.GetIdList())
        {
            if (skillTable.Get(id).SkillSortation == SkillSortationID.StageSkill)
                StageSkillList.Add(id);
        }
        StageSkillList.Add(character.BasicSkill);
    }

    // 레벨업 시 호출
    public void ShowLevelUpSkills()
    {
        var characterSkillList = character.GetSkillIdList();
        pendingSkillOptions = new List<int>();

        // 후보 3개 생성
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

        // 버튼 UI 업데이트
        for (int i = 0; i < 3; i++)
        {
            Buttons[i].gameObject.SetActive(true);

            TMP_Text tmpText = Buttons[i].GetComponentInChildren<TMP_Text>();
            Image img = Buttons[i].transform.Find("Icon").GetComponent<Image>();

            int skillId = pendingSkillOptions[i];
            SkillData s = DataTableManager.Get<SkillTable>("SkillTable").Get(skillId);

            int pickId = characterSkillList.Contains(skillId) ? Random.Range(1, 6) : 0;
            SkillSelectionData ss = null;
            if (pickId > 0)
                ss = DataTableManager.Get<SkillSelectionTable>("SkillSelectionTable").Get(skillId, pickId);

            // 텍스트 & 이미지
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
        if (index >= pendingSkillOptions.Count) return;

        int skillId = pendingSkillOptions[index];
        var characterSkillList = character.GetSkillIdList();
        var selectionTable = DataTableManager.Get<SkillSelectionTable>("SkillSelectionTable");

        if (characterSkillList.Contains(skillId))
        {
            int pickId = Random.Range(1, 6);
            SkillSelectionData skillData = selectionTable.Get(skillId, pickId);
            if (skillData != null)
                character.IncreaseSkill(skillId, skillData);
        }
        else
        {
            if (characterSkillList.Count < 5)
                character.AddSkill(skillId);
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

