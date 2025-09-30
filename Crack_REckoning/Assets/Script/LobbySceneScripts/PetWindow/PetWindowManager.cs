using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetWindowManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject PetEnforceWindow;

    [Header("Buttons")]
    public Button[] PetSelectButtons;
    public Button ExitButton;
    public Button PetEnforceButton;
    public Button PetEquipButton;

    [Header("Texts")]
    public TextMeshProUGUI PetName;
    public TextMeshProUGUI PetDescription;
    public TextMeshProUGUI[] PetStats;
    public TextMeshProUGUI EnforcePct;
    public TextMeshProUGUI EnforceDamage;
    public TextMeshProUGUI EnforceStat;
    public TextMeshProUGUI EnforceCurrentGold;
    public TextMeshProUGUI EnforceNeedGold;
    public TextMeshProUGUI Gold;

    [Header("Images")]
    public Image PetImage;

    private PetTable PetTable;
    private List<int> _owned;
    private int _selectedPetId = -1;

    [SerializeField] private ButtonAudio ButtonAudio;

    private void OnEnable()
    {
        if (PetEnforceWindow)
            PetEnforceWindow.SetActive(false);

        PetTable = DataTableManager.Get<PetTable>("PetTable");
        _owned = SaveLoadManager.Data != null ? SaveLoadManager.Data.OwnedPetIds : null;

        if (PetTable == null || _owned == null)
        {
            Debug.LogWarning("[PetWindow] 데이터가 준비되지 않았습니다.");
            return;
        }

        BindExit();
        BindPetButtons();
        _selectedPetId = -1;
    }

    private void OnDisable()
    {
        if (ExitButton) ExitButton.onClick.RemoveAllListeners();
        if (PetEnforceButton) PetEnforceButton.onClick.RemoveAllListeners();
        if (PetEquipButton) PetEquipButton.onClick.RemoveAllListeners();

        if (PetSelectButtons != null)
        {
            foreach (var b in PetSelectButtons)
                if (b) b.onClick.RemoveAllListeners();
        }
    }

    private void BindExit()
    {
        if (!ExitButton) return;
        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(() =>
        {
            if (PetEnforceWindow)
                PetEnforceWindow.SetActive(false);
        });
        ExitButton.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void BindPetButtons()
    {
        if (PetSelectButtons == null) return;

        int fillCount = Mathf.Min(PetSelectButtons.Length, _owned.Count);

        for (int i = 0; i < fillCount; i++)
            RebindButtonSlot(i);   // ← 슬롯별 바인딩

        for (int i = fillCount; i < PetSelectButtons.Length; i++)
        {
            var btn = PetSelectButtons[i];
            if (!btn) continue;
            if (btn.image) btn.image.enabled = false;
            btn.interactable = false;
            btn.onClick.RemoveAllListeners();
        }
    }
    private void RebindButtonSlot(int slot)
    {
        var btn = PetSelectButtons[slot];
        if (btn == null) return;

        int sid = _owned[slot];
        var sdata = PetTable.Get(sid);

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
            OnPetSelected(currentSid);
        });
        btn.onClick.AddListener(ButtonAudio.PlayClickSound);
    }

    private void OnPetSelected(int id)
    {
        _selectedPetId = id;
        if (PetEnforceWindow)
            PetEnforceWindow.SetActive(true);

        if (PetEnforceButton)
        {
            PetEnforceButton.onClick.RemoveAllListeners();
            PetEnforceButton.onClick.AddListener(TryEnforceSelected);
            PetEnforceButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        }
        if (PetEquipButton)
        {
            PetEquipButton.onClick.RemoveAllListeners();
            PetEquipButton.onClick.AddListener(TryEquipSelected);
            PetEquipButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        }

        RefreshUI(id);
    }

    private void TryEnforceSelected()
    {
        TryEnforce(_selectedPetId);
    }

    private void TryEquipSelected()
    {
        TryEquipPet(_selectedPetId);
    }

    public void TryEnforce(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null)
        {
            Debug.LogWarning("[PetWindow] SaveLoadManager.Data is null");
            return;
        }

        if (data.OwnedPetIds == null) data.OwnedPetIds = new List<int>();

        int ownedIdx = data.OwnedPetIds.IndexOf(currentId);
        if (ownedIdx < 0)
        {
            Debug.LogWarning($"[PetWindow] 보유하지 않은 펫 강화 시도: {currentId}");
            return;
        }

        var table = DataTableManager.Get<PetEnforceTable>(DataTableIds.PetEnforceTable);
        if (table == null)
        {
            Debug.LogWarning("[PetWindow] PetEnforceTable not found");
            return;
        }

        var row = table.Get(currentId);
        if (row == null)
        {
            Debug.Log("[PetWindow] 강화 최종 단계이거나 데이터 없음");
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
            data.OwnedPetIds[ownedIdx] = next;

            if (data.EquipmentPetId == currentId)
                data.EquipmentPetId = next;

            _selectedPetId = next;

            RebindButtonSlot(ownedIdx);

            RefreshUI(next);
        }
        else
        {
            RefreshUI(currentId);
        }

        SaveLoadManager.Save();
    }
    private void TryEquipPet(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null) return;

        data.EquipmentPetId = currentId;

        SaveLoadManager.Save();

        RefreshUI(currentId);
    }

    private void RefreshUI(int id)
    {
        var data = SaveLoadManager.Data;
        if (data == null || PetTable == null) return;

        var sdata = PetTable.Get(id);
        if (sdata == null) return;

        var table = DataTableManager.Get<PetEnforceTable>(DataTableIds.PetEnforceTable);
        var endata = table != null ? table.Get(id) : null;
        var hasNext = endata != null;
        var nextsdata = hasNext ? PetTable.Get(endata.ResultRewards) : null;

        if (PetImage)
        {
            if (sdata.sprite != null)
            {
                PetImage.enabled = true;
                PetImage.sprite = sdata.sprite;
            }
            else
            {
                PetImage.enabled = false;
            }
        }

        if (PetName) PetName.text = sdata.PetName;
        if (PetDescription) PetDescription.text = sdata.PetDesc;

        if (PetStats != null && PetStats.Length >= 1)
        {
            if (PetStats[0])
            {
                if(sdata.GoldUp > 0)
                {
                    PetStats[0].text = $"마석 획득량 증가\n{sdata.GoldUp}";
                }
                else if(sdata.WaveTime > 0)
                {
                    PetStats[0].text = $"웨이브 시간 감소\n{sdata.WaveTime}";
                }
                else if (sdata.WallHpUp > 0)
                {
                    PetStats[0].text = $"방벽 체력\n{sdata.WallHpUp}";
                }
                else if (sdata.AttBuff > 0)
                {
                    PetStats[0].text = $"추가 피해량\n{sdata.AttBuff}";
                }
            }
            
        }

        if (hasNext && nextsdata != null)
        {
            if (EnforcePct) EnforcePct.text = $"강화 확률 : <color=#7EE787>{endata.SuccessPercent}%</color>";
            if (EnforceDamage)
            {
                if (sdata.GoldUp < nextsdata.GoldUp)
                {
                    EnforceDamage.text = $"마석 획득량 증가\n{nextsdata.GoldUp - sdata.GoldUp:F2}";
                }
                if (sdata.WaveTime < nextsdata.WaveTime)
                {
                    EnforceDamage.text = $"웨이브 시간 감소\n{nextsdata.WaveTime - sdata.WaveTime}";
                }
                if (sdata.WallHpUp < nextsdata.WallHpUp)
                {
                    EnforceDamage.text = $"방벽체력 증가\n{nextsdata.WallHpUp - sdata.WallHpUp}";
                }
                if (sdata.AttBuff < nextsdata.AttBuff)
                {
                    EnforceDamage.text = $"추가 피해량 증가\n{nextsdata.AttBuff - sdata.AttBuff}";
                }
            }

            var sb = new StringBuilder();
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

        if (PetEnforceButton != null)
        {
            bool canEnhance = hasNext && data.Gold >= (endata?.GoldNum ?? int.MaxValue);
            PetEnforceButton.interactable = canEnhance;

            var cg = PetEnforceButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = PetEnforceButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEnhance ? 1f : 0.4f;
        }

        if (PetEquipButton != null)
        {
            bool canEquip = SaveLoadManager.Data != null
    && SaveLoadManager.Data.EquipmentPetId != id;
            PetEquipButton.interactable = canEquip;

            var cg = PetEquipButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = PetEquipButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canEquip ? 1f : 0.2f;
        }
    }
}