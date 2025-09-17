using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static readonly string SkillSelectionTable = "SkillSelectionTable";
    private static readonly string SkillTable = "SkillTable";

    public Character character;
    public SkillSelectionData skillSelectionData;
    public Button[] Buttons;

    private List<int> characterSkillList;
    private List<int> StageSkillList;

    public int SkillID { get; private set; }
    public int SkillPickID { get; private set; }
    public string SkillPickName { get; private set; }
    public float SkillDamageNumChange { get; private set; }
    public float IncreasingSkillDamageRange { get; private set; }
    public float ReduceSkillCT { get; private set; }
    public int IncreasedProjectile { get; private set; }
    public int IncreaseNumAttack { get; private set; }
    public int PeneTratingPower { get; private set; }
    public int EffectID { get; private set; }

    private void Awake()
    {
        character = GetComponent<Character>();
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
    }
    public void StageSkillListInit()
    {
        var skillTable = DataTableManager.Get<SkillTable>(SkillTable);
        var List = skillTable.GetIdList();
        foreach(var item in List)
        {
            if(skillTable.Get(item).SkillSortation == SkillSortationID.StageSkill)
            {
                StageSkillList.Add(item);
            }
        }
    }
    public void RandSkillSet()
    {
        characterSkillList = character.GetSkillIdList();
        
    }

    public void ButtonsSet()
    {
        for (int i = 0; i< Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(true);
        }
    }

    public void SelectSkill(int skillId, int pickId)
    {
        var selectionTable = DataTableManager.Get<SkillSelectionTable>(SkillSelectionTable);
        skillSelectionData = selectionTable.Get(skillId, pickId);

        if (skillSelectionData == null)
        {
            Debug.LogError($"스킬 {skillId}, Pick {pickId} 데이터가 없음!");
            return;
        }
        SkillID = skillId;
        SkillPickID = pickId;
        SkillPickName = skillSelectionData.SkillPickName;
        SkillDamageNumChange = skillSelectionData.SkillDamageNumChange;
        IncreasingSkillDamageRange = skillSelectionData.IncreasingSkillDamageRange.GetValueOrDefault();
        ReduceSkillCT = skillSelectionData.ReduceSkillCT.GetValueOrDefault();
        IncreasedProjectile = skillSelectionData.IncreasedProjectile.GetValueOrDefault();
        IncreaseNumAttack = skillSelectionData.IncreaseNumAttack.GetValueOrDefault();
        PeneTratingPower = skillSelectionData.PeneTratingPower.GetValueOrDefault();
        EffectID = skillSelectionData.EffectID.GetValueOrDefault();
    }
}