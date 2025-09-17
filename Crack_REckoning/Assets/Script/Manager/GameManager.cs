using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Character character;
    public Button[] Buttons; // 3개 버튼
    private List<int> StageSkillList;          // 스테이지에서 나올 수 있는 스킬 ID 목록
    private List<int> pendingSkillOptions;     // 현재 레벨업 선택 후보 스킬

    private void Awake()
    {
        StageSkillList = new List<int>();
        StageSkillListInit();

        // 버튼 숨김 + 클릭 이벤트 연결
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            int index = i; // 캡처용
            Buttons[i].onClick.AddListener(() => OnSkillButtonClick(index));
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
                // 기존 스킬 강화 후보
                if (!pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
            else
            {
                // 새로운 스킬은 5개 미만일 때만 후보 가능
                if (characterSkillList.Count < 5 && !pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
        }

        // 버튼 UI 업데이트
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(true);

            TMP_Text tmpText = Buttons[i].GetComponentInChildren<TMP_Text>();
            Image img = Buttons[i].transform.Find("Icon").GetComponent<Image>();

            int skillId = pendingSkillOptions[i];
            SkillData s = DataTableManager.Get<SkillTable>("SkillTable").Get(skillId);

            // pickId 결정: 기존 스킬이면 1~5 랜덤, 새 스킬이면 0
            int pickId = characterSkillList.Contains(skillId) ? Random.Range(1, 6) : 0;
            SkillSelectionData ss = null;
            if (pickId > 0)
                ss = DataTableManager.Get<SkillSelectionTable>("SkillSelectionTable").Get(skillId, pickId);

            // 텍스트 & 이미지
            if (tmpText != null)
                tmpText.text = (ss != null) ? ss.SkillPickName : s.SkillName; // 새 스킬이면 그냥 이름
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
            // 기존 스킬 강화
            int pickId = Random.Range(1, 6); // CSV에는 1~5
            SkillSelectionData skillData = selectionTable.Get(skillId, pickId);
            if (skillData != null)
                character.IncreaseSkill(skillId, skillData);
        }
        else
        {
            // 스킬 슬롯이 5개 미만일 때만 추가
            if (characterSkillList.Count < 5)
                character.AddSkill(skillId);
        }

        // 버튼 숨기고 게임 재개
        foreach (var btn in Buttons)
            btn.gameObject.SetActive(false);

        ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}

