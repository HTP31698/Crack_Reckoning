using System.Collections.Generic;
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

    private SkillTable _skillTable;
    private List<int> _owned;
    private int _selectedSkillId = -1;

    private void OnEnable()
    {
        // 패널 열릴 때마다 초기화
        SkillEnforceWindow.SetActive(false);

        // 데이터 확보 (Awake보다 OnEnable이 안전한 편)
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
        // 리스너 정리 (중복 방지)
        if (ExitButton) ExitButton.onClick.RemoveAllListeners();
        if (SkillEnforceButton) SkillEnforceButton.onClick.RemoveAllListeners();
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
        ExitButton.onClick.AddListener(() => SkillEnforceWindow.SetActive(false));
    }

    private void BindSkillButtons()
    {
        if (SkillSelectButtons == null) return;

        int fillCount = Mathf.Min(SkillSelectButtons.Length, _owned.Count);

        for (int i = 0; i < fillCount; i++)
        {
            var btn = SkillSelectButtons[i];
            if (btn == null || btn.image == null) continue;

            int skillId = _owned[i];          // 캡처용 로컬 변수

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

            // 중복 방지 후 리스너 등록
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSkillSelected(skillId));
        }

        // 남는 버튼 정리
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
        SkillEnforceWindow.SetActive(true);

        // 강화 버튼 리스너 갱신 (항상 1개만 있도록)
        if (SkillEnforceButton)
        {
            SkillEnforceButton.onClick.RemoveAllListeners();
            SkillEnforceButton.onClick.AddListener(TryEnforceSelected);
        }

        // 테스트용 골드 세팅이면 UI 전에 해두는 게 자연스러움
        SaveLoadManager.Data.Gold = 99999999;

        RefreshUI();
        var sdata = _skillTable.Get(id);
        if (sdata != null) Debug.Log($"스킬 선택: {sdata.SkillName} (id={id})");
    }

    private void TryEnforceSelected()
    {
        if (_selectedSkillId < 0)
        {
            Toast("선택된 스킬이 없습니다.");
            return;
        }

        TryEnforce(_selectedSkillId);
    }

    public void TryEnforce(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null) { Toast("데이터 없음"); return; }

        var table = DataTableManager.Get<SkillEnforceTable>(DataTableIds.SkillEnforceTable);
        if (table == null) { Toast("강화 테이블 없음"); return; }

        var row = table.Get(currentId);
        if (row == null) { Toast("최대 강화입니다"); return; }

        int next = row.ResultRewards;
        int cost = row.GoldNum;
        int pct = Mathf.Clamp(row.SuccessPercent, 0, 100);

        if (data.OwnedSkillIds == null) data.OwnedSkillIds = new List<int>();
        if (data.EquipmentSkillIds == null) data.EquipmentSkillIds = new List<int>();

        int ownedIdx = data.OwnedSkillIds.IndexOf(currentId);
        if (ownedIdx < 0) { Toast("보유하지 않은 스킬입니다."); return; }

        if (data.Gold < cost) { Toast("골드가 부족합니다."); return; }
        data.Gold -= cost;

        bool success = Random.Range(0, 100) < pct; // int 버전은 상한 미포함(0~99)

        if (success)
        {
            data.OwnedSkillIds[ownedIdx] = next;

            // 2) 장착 목록에서 첫 슬롯만 교체 (없으면 아무 것도 안 함)
            int equipIdx = data.EquipmentSkillIds.IndexOf(currentId); // 첫 번째만 찾음
            if (equipIdx >= 0)
            {
                data.EquipmentSkillIds[equipIdx] = next;  // ← 여기서 딱 한 칸만 바꾼다!
            }
            _selectedSkillId = next; // 선택 ID 갱신(옵션)
            Toast($"강화 성공! {row.EnhanceID} → {next}");
        }
        else
        {
            Toast("강화 실패");
        }

        // 장착 슬롯 보정(필요 시)
        const int MaxSlots = 5;
        while (data.EquipmentSkillIds.Count < MaxSlots) data.EquipmentSkillIds.Add(0);
        if (data.EquipmentSkillIds.Count > MaxSlots)
            data.EquipmentSkillIds.RemoveRange(MaxSlots, data.EquipmentSkillIds.Count - MaxSlots);

        SaveLoadManager.Save();

        // 버튼/아이콘/골드 등 갱신
        RefreshUI();

        // ★ 재귀 금지: 여기서 다시 OnSkillSelected/리스너 추가하지 않음
        // 필요하면 버튼 스프라이트만 갱신:
        // BindSkillButtons(); // 스프라이트 전체 리바인딩이 필요할 때만 호출
    }

    private void Toast(string msg) { Debug.Log("[Enforce] " + msg); }
    private void RefreshUI() { /* 골드 텍스트/선택 아이콘/확률/비용 등 갱신 */ }
}

