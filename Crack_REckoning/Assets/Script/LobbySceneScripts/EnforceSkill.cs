using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnforceSkill : MonoBehaviour
{
    public Button SelectSkillButton;
    public Button SkillEnforceButton;


    public void TryEnforce(int currentId)
    {
        var data = SaveLoadManager.Data;
        if (data == null) { Toast("데이터 없음");return; }

        var table = DataTableManager.Get<SkillEnforceTable>(DataTableIds.SkillEnforceTable);
        if (table == null) { Toast("강화 테이블 없음"); return; }

        // 1) 테이블에서 현재 ID에 대한 행 찾기
        var row = table.Get(currentId);
        if (row == null) { Toast("강화 데이터 없음"); return; }
        if (row.ResultRewards <= 0) { Toast("최대 강화입니다.");return; }

        int next = row.ResultRewards;
        int cost = row.GoldNum;
        int pct = row.SuccessPercent;

        // 2) 컬렉션 null 가드
        if (data.OwnedSkillIds == null) data.OwnedSkillIds = new List<int>();
        if (data.EquipmentSkillIds == null) data.EquipmentSkillIds = new List<int>();

        // 3) 보유 여부 확인
        int ownedIdx = data.OwnedSkillIds.IndexOf(currentId);
        if (ownedIdx < 0) { Toast("보유하지 않은 스킬입니다."); return; }

        // 4) 골드 체크
        if (data.Gold < cost) { Toast("골드가 부족합니다."); return; }
        data.Gold -= cost; // 비용 차감

        // 5) 확률 계산 (0~100 → 0~1로)
        if (pct < 0) pct = 0;
        if (pct > 100) pct = 100;

        bool success = Random.Range(0, 100) < pct;

        if (success)
        {
            // 6-a) 성공: 보유 리스트에서 같은 자리 교체
            data.OwnedSkillIds[ownedIdx] = next;

            // 장착 중이면 같은 슬롯에서 교체
            int equipIdx = data.EquipmentSkillIds.IndexOf(currentId);
            if (equipIdx >= 0) data.EquipmentSkillIds[equipIdx] = next;

            Toast("강화 성공!");
        }
        else
        {
            // 6-b) 실패: 유지(원하면 하락/파괴 정책 추가)
            Toast("강화 실패");
        }

        // 7) 슬롯 길이 보정(선택) — 장착은 보통 5칸 고정
        const int MaxSlots = 5;
        while (data.EquipmentSkillIds.Count < MaxSlots) data.EquipmentSkillIds.Add(0);
        if (data.EquipmentSkillIds.Count > MaxSlots)
            data.EquipmentSkillIds.RemoveRange(MaxSlots, data.EquipmentSkillIds.Count - MaxSlots);

        // 8) 저장 + UI 갱신
        SaveLoadManager.Save();
        RefreshUI();
    }

    private void Toast(string msg) { Debug.Log("[Enforce] " + msg); }
    private void RefreshUI() { /* 골드/리스트/버튼/아이콘 갱신 코드 */ }
}

