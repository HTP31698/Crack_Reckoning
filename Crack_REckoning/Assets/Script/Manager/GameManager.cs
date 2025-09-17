using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Character character;
    public Button[] Buttons; // 3�� ��ư
    private List<int> StageSkillList;          // ������������ ���� �� �ִ� ��ų ID ���
    private List<int> pendingSkillOptions;     // ���� ������ ���� �ĺ� ��ų

    private void Awake()
    {
        StageSkillList = new List<int>();
        StageSkillListInit();

        // ��ư ���� + Ŭ�� �̺�Ʈ ����
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            int index = i; // ĸó��
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

    // ������ �� ȣ��
    public void ShowLevelUpSkills()
    {
        var characterSkillList = character.GetSkillIdList();
        pendingSkillOptions = new List<int>();

        // �ĺ� 3�� ����
        while (pendingSkillOptions.Count < 3)
        {
            int randSkill = StageSkillList[Random.Range(0, StageSkillList.Count)];

            if (characterSkillList.Contains(randSkill))
            {
                // ���� ��ų ��ȭ �ĺ�
                if (!pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
            else
            {
                // ���ο� ��ų�� 5�� �̸��� ���� �ĺ� ����
                if (characterSkillList.Count < 5 && !pendingSkillOptions.Contains(randSkill))
                    pendingSkillOptions.Add(randSkill);
            }
        }

        // ��ư UI ������Ʈ
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(true);

            TMP_Text tmpText = Buttons[i].GetComponentInChildren<TMP_Text>();
            Image img = Buttons[i].transform.Find("Icon").GetComponent<Image>();

            int skillId = pendingSkillOptions[i];
            SkillData s = DataTableManager.Get<SkillTable>("SkillTable").Get(skillId);

            // pickId ����: ���� ��ų�̸� 1~5 ����, �� ��ų�̸� 0
            int pickId = characterSkillList.Contains(skillId) ? Random.Range(1, 6) : 0;
            SkillSelectionData ss = null;
            if (pickId > 0)
                ss = DataTableManager.Get<SkillSelectionTable>("SkillSelectionTable").Get(skillId, pickId);

            // �ؽ�Ʈ & �̹���
            if (tmpText != null)
                tmpText.text = (ss != null) ? ss.SkillPickName : s.SkillName; // �� ��ų�̸� �׳� �̸�
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
            // ���� ��ų ��ȭ
            int pickId = Random.Range(1, 6); // CSV���� 1~5
            SkillSelectionData skillData = selectionTable.Get(skillId, pickId);
            if (skillData != null)
                character.IncreaseSkill(skillId, skillData);
        }
        else
        {
            // ��ų ������ 5�� �̸��� ���� �߰�
            if (characterSkillList.Count < 5)
                character.AddSkill(skillId);
        }

        // ��ư ����� ���� �簳
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

