// UIManagers.cs — UI 골격 묶음 (문서 06-1). 실제 위젯 와이어링은 프리팹 단계.
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Dungeon;

namespace BroDungeon.UI
{
    /// 메뉴 플로우: 타이틀→거점→던전 일시정지 (문서 06-1).
    public class MenuManager : MonoBehaviour
    {
        public GameObject titlePanel, hubMenuPanel, pausePanel;
        public void ShowTitle() => Toggle(titlePanel);
        public void ShowHubMenu() => Toggle(hubMenuPanel);
        public void TogglePause() { if (pausePanel) pausePanel.SetActive(!pausePanel.activeSelf); }
        void Toggle(GameObject p) { if (p) p.SetActive(true); }
    }

    /// 인벤토리 화면: A/B 탭, 슬롯, 보관함 그리드(5×8) (문서 06-1).
    public class InventoryUI : MonoBehaviour
    {
        public Transform aSlots, bSlots, storageGrid;
        public void Refresh() { /* InventorySystem/EquipmentSystem 바인딩 */ }
    }

    /// 제단 UI: 3택 카드 (문서 06-1).
    public class AltarUI : MonoBehaviour
    {
        public AltarSystem altar;
        public Transform cardRoot;
        List<AltarOption> _current;

        public void Open(int seed)
        {
            _current = altar.Generate(seed);
            // 카드 3개 표시: 이름/효과/비용/선택/건너뛰기
        }
        public void Pick(int index)
        {
            if (_current != null && index < _current.Count && altar.Choose(_current[index]))
                gameObject.SetActive(false);
        }
        public void Skip() => gameObject.SetActive(false);
    }

    /// 룬 조합 UI: 2슬롯 배치판 (문서 06-1).
    public class RuneCraftUI : MonoBehaviour
    {
        public void Combine(BroDungeon.Data.AttributeType a, BroDungeon.Data.AttributeType b)
        {
            var combo = Items.RuneSystem.Instance?.TryCombo(a, b);
            // 결과 미리보기/발견 표시
        }
    }

    /// 미니맵: 방 구조/클리어/미탐험 (문서 06-1).
    public class MinimapUI : MonoBehaviour
    {
        public RoomManager rooms;
        public void Refresh() { /* rooms.Current.Rooms 순회 그리기 */ }
    }

    /// 대화 UI: 캠프파이어 7단계/말풍선 (문서 05-3).
    public class DialogueUI : MonoBehaviour
    {
        public void Show(string speaker, string line) { }
    }

    /// 접근성 설정 UI (문서 06-6, 24종).
    public class AccessibilityUI : MonoBehaviour
    {
        public Data.SettingsData Settings => Core.SaveManager.Instance?.Data.permanent.settings;
        public void Apply() { /* AudioManager/카메라/입력에 반영 */ }
    }
}
