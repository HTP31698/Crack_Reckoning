using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillWindowManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject SkillEnforceWindow;

    [Header("Buttons")]
    public Button[] SkillSelectButtons;
    public Button ExitButton;
    public Button SkillEnforceButton;
    public Button SkillEquipButton;

    [Header("Texts")]
    public TextMeshProUGUI SkillName;
    public TextMeshProUGUI SkillDescription;
    public TextMeshProUGUI[] SkillStats;
    public TextMeshProUGUI EnforcePct;
    public TextMeshProUGUI EnforceDamage;
    public TextMeshProUGUI EnforceStat;
    public TextMeshProUGUI EnforceCurrentGold;
    public TextMeshProUGUI EnforceNeedGold;
    public TextMeshProUGUI Gold;

    public Image SkillImage;

    private SkillTable _skillTable;
    private List<int> _owned;
    private int _selectedSkillId = -1;

    private void OnEnable()
    {
        if (SkillEnforceWindow) SkillEnforceWindow.SetActive(false);

        _skillTable = DataTableManager.Get<SkillTable>("SkillTable");
        _owned = SaveLoadManager.Data != null ? SaveLoadManager.Data.OwnedSkillIds : null;

        if (_skillTable == null || _owned == null)
        {
            Debug.LogWarning("[SkillWindow] 데이터가 준비되지 않았습니다.");
            return;
        }

        BindExit();
        BindSkillButtons();
        _selectedSkillId = -1;
    }

    private void OnDisable()
    {
        if (ExitButton) ExitButton.onClick.RemoveAllListeners();
        if (SkillEnforceButton) SkillEnforceButton.onClick.RemoveAllListeners();
        if (SkillEquipButton) SkillEquipButton.onClick.RemoveAllListeners();

        if (SkillSelectButtons != null)
        {
            foreach (var b in SkillSelectButtons)
                if (b) b.onClick.RemoveAllListeners();
        }
    }

    private void BindExit()
    {
        if (!ExitButton) return;
        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(() =>
        {
            if (SkillEnforceWindow) SkillEnforceWindow.SetActive(false);
        });
    }

    private void BindSkillButtons()
    {
        if (SkillSelectButtons == null) return;

        int fillCount = Mathf.Min(SkillSelectButtons.Length, _owned.Count);

        for (int i = 0; i < fillCount; i++)
        {
            var btn = SkillSelectButtons[i];
            if (btn == null || btn.image == null) continue;

            int skillId = _owned[i];

            var sdata = _skillTable.Get(skillId);
            if (sdata == null || sdata.sprite == null)
            {
                btn.image.enabled = false;
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
                continue;
            }

            btn.image.enabled = true;
            btn.image.sprite = sdata.sprite;
            btn.interactable = true;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSkillSelected(skillId));
        }

        for (int i = fillCount; i < SkillSelectButtons.Length; i++)
        {
            var btn = SkillSelectButtons[i];
            if (!btn) continue;
            if (btn.image) btn.image.enabled = false;
            btn.interactable = false;
            btn.onClick.RemoveAllListeners();
        }
    }

    private void OnSkillSelected(int id)
    {
        _selectedSkillId = id;
        if (SkillEnforceWindow) SkillEnforceWindow.SetActive(true);

        if (SkillEnforceButton)
        {
            SkillEnforceButton.onClick.RemoveAllListeners();
            SkillEnforceButton.onClick.AddListener(TryEnforceSelected);
        }
        if (SkillEquipButton)
        {
            SkillEquipButton.onClick.RemoveAllListeners();
            SkillEquipButton.onClick.AddListener(TryEquipSelected);
        }

        // 테스트 코드는 개발 중에만
        // SaveLoadManager.Data.Gold = 99999999;

        RefreshUI(id);
    }

    private void TryEnforceSelected()
    {
        TryEnforce(_selectedSkillId);
    }

    private void TryEquipSelected()
    {
        TryEquipSkill(_selectedSkillId);
    }

    public void TryEnforce(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null)
        {
            Debug.LogWarning("[SkillWindow] SaveLoadManager.Data is null");
            return;
        }

        if (data.OwnedSkillIds == null) data.OwnedSkillIds = new List<int>();
        if (data.EquipmentSkillIds == null) data.EquipmentSkillIds = new List<int>();

        int ownedIdx = data.OwnedSkillIds.IndexOf(currentId);
        if (ownedIdx < 0)
        {
            Debug.LogWarning($"[SkillWindow] 보유하지 않은 스킬 강화 시도: {currentId}");
            return;
        }

        var table = DataTableManager.Get<SkillEnforceTable>(DataTableIds.SkillEnforceTable);
        if (table == null)
        {
            Debug.LogWarning("[SkillWindow] SkillEnforceTable not found");
            return;
        }

        var row = table.Get(currentId);
        if (row == null)
        {
            Debug.Log("[SkillWindow] 강화 최종 단계이거나 데이터 없음");
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

        // 비용 선차감 (대부분 게임이 이렇게 처리)
        data.Gold -= cost;

        bool success = Random.Range(0, 100) < pct; // 0~99
        if (success)
        {
            data.OwnedSkillIds[ownedIdx] = next;

            int equipIdx = data.EquipmentSkillIds.IndexOf(currentId);
            if (equipIdx >= 0)
                data.EquipmentSkillIds[equipIdx] = next;

            _selectedSkillId = next;
            RefreshUI(next);
        }
        else
        {
            RefreshUI(currentId);
        }

        SaveLoadManager.Save();
    }
    private void TryEquipSkill(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null) return;

        if (data.EquipmentSkillIds == null)
            data.EquipmentSkillIds = new List<int>();

        data.EquipmentSkillIds.Clear();
        data.EquipmentSkillIds.Add(currentId);

        SaveLoadManager.Save();

        if (SkillEquipButton) SkillEquipButton.interactable = false;
    }

    private void RefreshUI(int id)
    {
        var data = SaveLoadManager.Data;
        if (data == null || _skillTable == null) return;

        var sdata = _skillTable.Get(id);
        if (sdata == null) return;

        var table = DataTableManager.Get<SkillEnforceTable>(DataTableIds.SkillEnforceTable);
        var endata = table != null ? table.Get(id) : null;
        var hasNext = endata != null;
        var nextsdata = hasNext ? _skillTable.Get(endata.ResultRewards) : null;

        if (SkillImage)
        {
            if (sdata.sprite != null)
            {
                SkillImage.enabled = true;
                SkillImage.sprite = sdata.sprite;
            }
            else
            {
                SkillImage.enabled = false;
            }
        }

        if (SkillName) SkillName.text = sdata.SkillName;
        if (SkillDescription) SkillDescription.text = sdata.SkillDescription;

        if (SkillStats != null && SkillStats.Length >= 6)
        {
            if (SkillStats[0]) SkillStats[0].text = $"공격력\n{sdata.SkillDamage}";
            if (SkillStats[1]) SkillStats[1].text = $"쿨타임\n{sdata.SkillCoolTime}";
            if (SkillStats[2]) SkillStats[2].text = $"투사체 개수\n{sdata.ProjectilesNum}";
            if (SkillStats[3]) SkillStats[3].text = $"공격 횟수\n{sdata.AttackNum}";
            if (SkillStats[4]) SkillStats[4].text = $"관통력\n{sdata.PenetratingPower}";
            if (SkillStats[5]) SkillStats[5].text = (sdata.AttackType == AttackTypeID.Projectile) ? "투사체 공격" : "범위 공격";
        }

        if (hasNext && nextsdata != null)
        {
            if (EnforcePct) EnforcePct.text = $"강화 확률 : <color=#7EE787>{endata.SuccessPercent}%</color>";
            if (EnforceDamage) EnforceDamage.text = $"공격력 증가 : +{nextsdata.SkillDamage - sdata.SkillDamage}";

            var sb = new StringBuilder();
            if (nextsdata.SkillCoolTime < sdata.SkillCoolTime)
                sb.AppendLine($"쿨타임 감소 : {sdata.SkillCoolTime - nextsdata.SkillCoolTime}");
            if (nextsdata.ProjectilesNum > sdata.ProjectilesNum)
                sb.AppendLine($"투사체 증가 : +{nextsdata.ProjectilesNum - sdata.ProjectilesNum}");
            if (nextsdata.AttackNum > sdata.AttackNum)
                sb.AppendLine($"공격 횟수 증가 : +{nextsdata.AttackNum - sdata.AttackNum}");
            if (nextsdata.PenetratingPower > sdata.PenetratingPower)
                sb.AppendLine($"관통력 증가 : +{nextsdata.PenetratingPower - sdata.PenetratingPower}");

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

        // 강화 버튼 활성/비활성
        if (SkillEnforceButton)
        {
            bool canEnhance = hasNext && data.Gold >= (endata?.GoldNum ?? int.MaxValue);
            SkillEnforceButton.interactable = canEnhance;

            var cg = SkillEquipButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = SkillEnforceButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEnhance ? 1f : 0.9f;
        }

        // 장착 버튼 활성/비활성 + 비활성 시 투명도 0.5
        if (SkillEquipButton)
        {
            bool canEquip = (SaveLoadManager.Data?.EquipmentSkillIds == null)
                            || !SaveLoadManager.Data.EquipmentSkillIds.Contains(id);

            SkillEquipButton.interactable = canEquip;

            var cg = SkillEquipButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = SkillEquipButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEquip ? 1f : 0.5f;
        }
    }


}