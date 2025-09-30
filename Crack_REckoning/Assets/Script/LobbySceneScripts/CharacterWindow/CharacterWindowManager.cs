using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWindowManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject CharacterEnforceWindow;

    [Header("Buttons")]
    public Button[] CharacterSelectButtons;
    public Button ExitButton;
    public Button CharacterEnforceButton;
    public Button CharacterEquipButton;

    [Header("Texts")]
    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CharacterDescription;
    public TextMeshProUGUI[] CharacterStats;
    public TextMeshProUGUI EnforcePct;
    public TextMeshProUGUI EnforceDamage;
    public TextMeshProUGUI EnforceStat;
    public TextMeshProUGUI EnforceCurrentGold;
    public TextMeshProUGUI EnforceNeedGold;
    public TextMeshProUGUI Gold;

    [Header("Images")]
    public Image CharacterImage;

    private CharacterTable CharacterTable;
    private List<int> _owned;
    private int _selectedCharacterId = -1;

    private void OnEnable()
    {
        if (CharacterEnforceWindow)
            CharacterEnforceWindow.SetActive(false);

        CharacterTable = DataTableManager.Get<CharacterTable>("CharacterTable");
        _owned = SaveLoadManager.Data != null ? SaveLoadManager.Data.OwnedCharacterIds : null;

        if (CharacterTable == null || _owned == null)
        {
            Debug.LogWarning("[CharacterWindow] 데이터가 준비되지 않았습니다.");
            return;
        }

        BindExit();
        BindCharacterButtons();
        _selectedCharacterId = -1;
    }

    private void OnDisable()
    {
        if (ExitButton) ExitButton.onClick.RemoveAllListeners();
        if (CharacterEnforceButton) CharacterEnforceButton.onClick.RemoveAllListeners();
        if (CharacterEquipButton) CharacterEquipButton.onClick.RemoveAllListeners();

        if (CharacterSelectButtons != null)
        {
            foreach (var b in CharacterSelectButtons)
                if (b) b.onClick.RemoveAllListeners();
        }
    }

    private void BindExit()
    {
        if (!ExitButton) return;
        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(() =>
        {
            if (CharacterEnforceWindow) 
                CharacterEnforceWindow.SetActive(false);
        });
    }

    private void BindCharacterButtons()
    {
        if (CharacterSelectButtons == null) return;

        int fillCount = Mathf.Min(CharacterSelectButtons.Length, _owned.Count);

        for (int i = 0; i < fillCount; i++)
            RebindButtonSlot(i);   // ← 슬롯별 바인딩

        for (int i = fillCount; i < CharacterSelectButtons.Length; i++)
        {
            var btn = CharacterSelectButtons[i];
            if (!btn) continue;
            if (btn.image) btn.image.enabled = false;
            btn.interactable = false;
            btn.onClick.RemoveAllListeners();
        }
    }
    private void RebindButtonSlot(int slot)
    {
        var btn = CharacterSelectButtons[slot];
        if (btn == null) return;

        int sid = _owned[slot];
        var sdata = CharacterTable.Get(sid);

        if (sdata == null || sdata.sprite == null)
        {
            if (btn.image) btn.image.enabled = false;
            btn.interactable = false;
            btn.onClick.RemoveAllListeners();
            return;
        }

        if (btn.image)
        {
            btn.image.enabled = true;
            btn.image.sprite = sdata.sprite;
        }

        btn.interactable = true;
        btn.onClick.RemoveAllListeners();

        int capturedSlot = slot;
        btn.onClick.AddListener(() =>
        {
            int currentSid = _owned[capturedSlot];
            OnCharacterSelected(currentSid);
        });
    }

    private void OnCharacterSelected(int id)
    {
        _selectedCharacterId = id;
        if (CharacterEnforceWindow) 
            CharacterEnforceWindow.SetActive(true);

        if (CharacterEnforceButton)
        {
            CharacterEnforceButton.onClick.RemoveAllListeners();
            CharacterEnforceButton.onClick.AddListener(TryEnforceSelected);
        }
        if (CharacterEquipButton)
        {
            CharacterEquipButton.onClick.RemoveAllListeners();
            CharacterEquipButton.onClick.AddListener(TryEquipSelected);
        }

        RefreshUI(id);
    }

    private void TryEnforceSelected()
    {
        TryEnforce(_selectedCharacterId);
    }

    private void TryEquipSelected()
    {
        TryEquipCharacter(_selectedCharacterId);
    }

    public void TryEnforce(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null)
        {
            Debug.LogWarning("[CharacterWindow] SaveLoadManager.Data is null");
            return;
        }

        if (data.OwnedCharacterIds == null) data.OwnedCharacterIds = new List<int>();

        int ownedIdx = data.OwnedCharacterIds.IndexOf(currentId);
        if (ownedIdx < 0)
        {
            Debug.LogWarning($"[CharacterWindow] 보유하지 않은 캐릭터 강화 시도: {currentId}");
            return;
        }

        var table = DataTableManager.Get<CharacterEnforceTable>(DataTableIds.CharacterEnforceTable);
        if (table == null)
        {
            Debug.LogWarning("[CharacterWindow] CharacterEnforceTable not found");
            return;
        }

        var row = table.Get(currentId);
        if (row == null)
        {
            Debug.Log("[CharacterWindow] 강화 최종 단계이거나 데이터 없음");
            RefreshUI(currentId);
            return;
        }

        int next = row.ResultRewards;
        int cost = Mathf.Max(0, row.GoldNum);
        int pct = Mathf.Clamp(row.SuccessPercent, 0, 100);

        if (data.Gold < cost)
        {
            Debug.Log("골드 부족");
            RefreshUI(currentId);
            return;
        }

        data.Gold -= cost;

        bool success = Random.Range(0, 100) < pct;
        if (success)
        {
            data.OwnedCharacterIds[ownedIdx] = next;

            int equipIdx = data.PlayerID;
            if (equipIdx >= 0) 
                data.PlayerID = next;

            _selectedCharacterId = next;

            RebindButtonSlot(ownedIdx);

            RefreshUI(next);
        }
        else
        {
            RefreshUI(currentId);
        }

        SaveLoadManager.Save();
    }
    private void TryEquipCharacter(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null) return;

        data.PlayerID = currentId;

        SaveLoadManager.Save();

        RefreshUI(currentId);
    }

    private void RefreshUI(int id)
    {
        var data = SaveLoadManager.Data;
        if (data == null || CharacterTable == null) return;

        var sdata = CharacterTable.Get(id);
        if (sdata == null) return;

        var table = DataTableManager.Get<CharacterEnforceTable>(DataTableIds.CharacterEnforceTable);
        var endata = table != null ? table.Get(id) : null;
        var hasNext = endata != null;
        var nextsdata = hasNext ? CharacterTable.Get(endata.ResultRewards) : null;

        if (CharacterImage)
        {
            if (sdata.sprite != null)
            {
                CharacterImage.enabled = true;
                CharacterImage.sprite = sdata.sprite;
            }
            else
            {
                CharacterImage.enabled = false;
            }
        }

        if (CharacterName) CharacterName.text = sdata.ChName;
        if (CharacterDescription) CharacterDescription.text = sdata.ChDesc;

        if (CharacterStats != null && CharacterStats.Length >= 2)
        {
            if (CharacterStats[0]) CharacterStats[0].text = $"크리티컬 확률\n{sdata.ChCri}";
            if (CharacterStats[1]) CharacterStats[1].text = $"크리티컬 배율\n{sdata.ChCriDam}";
        }

        if (hasNext && nextsdata != null)
        {
            if (EnforcePct) EnforcePct.text = $"강화 확률 : <color=#7EE787>{endata.SuccessPercent}%</color>";
            if (EnforceDamage) EnforceDamage.text = $"크리티컬 확률 증가 : +{nextsdata.ChCri - sdata.ChCri}%";
            
            var sb = new StringBuilder();
            if (nextsdata.ChCriDam < sdata.ChCriDam)
                sb.AppendLine($"크리티컬 배율 증가 : {nextsdata.ChCriDam - sdata.ChCriDam}");
            if (EnforceStat) EnforceStat.text = sb.Length > 0 ? sb.ToString().TrimEnd() : "추가 강화 효과 변화 없음";


            if (EnforceCurrentGold)
            {
                if (data.Gold >= endata.GoldNum)
                    EnforceCurrentGold.text = $"<color=#7EE787>{data.Gold}</color>";
                else if (data.Gold < endata.GoldNum)
                    EnforceCurrentGold.text = $"<color=#FF2000>{data.Gold}</color>";
            }

            if (EnforceNeedGold)
            { EnforceNeedGold.text = $"<color=#6D8AA5>/</color>{endata.GoldNum}"; }

        }
        else
        {
            if (EnforcePct) EnforcePct.text = "-";
            if (EnforceDamage) EnforceDamage.text = "최대 레벨";
            if (EnforceStat) EnforceStat.text = "최대 레벨";
            if (EnforceNeedGold) EnforceNeedGold.text = "";
            if (EnforceCurrentGold) EnforceCurrentGold.text = $"{data.Gold}";
        }

        if (Gold) Gold.text = $"{data.Gold}";

        if (CharacterEnforceButton != null)
        {
            bool canEnhance = hasNext && data.Gold >= (endata?.GoldNum ?? int.MaxValue);
            CharacterEnforceButton.interactable = canEnhance;

            var cg = CharacterEnforceButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = CharacterEnforceButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEnhance ? 1f : 0.4f;
        }

        if (CharacterEquipButton != null)
        {
            bool canEquip = (SaveLoadManager.Data?.PlayerID == null);
            CharacterEquipButton.interactable = canEquip;

            var cg = CharacterEquipButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = CharacterEquipButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEquip ? 1f : 0.2f;
        }
    }
}