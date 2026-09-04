// NPCManager.cs — 거점 NPC 5종 + 호감도 (문서 05-3).
using System.Collections.Generic;
using UnityEngine;

namespace BroDungeon.Hub
{
    public class NPCDef
    {
        public string Id, Name, UnlockCondition, Role;
        public NPCDef(string id, string n, string cond, string role)
        { Id = id; Name = n; UnlockCondition = cond; Role = role; }
    }

    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        public static readonly List<NPCDef> All = new List<NPCDef>
        {
            new NPCDef("NPC_Merchant","방랑 상인","2구역 구출","희귀 아이템/재료 판매(런마다 갱신)"),
            new NPCDef("NPC_Warrior","노련한 전사","3구역 구출","특수 콤보 교습"),
            new NPCDef("NPC_Scholar","떠돌이 학자","4구역 구출","보스 약점, 퍼즐 힌트"),
            new NPCDef("NPC_Smith","저주받은 대장장이","3구역 엘리트","저주 장비 제작"),
            new NPCDef("NPC_Cat","미궁의 고양이","1구역 숨겨진 방","쓰다듬기, 행운↑"),
        };

        readonly Dictionary<string, int> _affinity = new Dictionary<string, int>();
        readonly HashSet<string> _rescued = new HashSet<string>();

        public bool IsRescued(string id) => _rescued.Contains(id);
        public void Rescue(string id) { _rescued.Add(id); }

        public int Affinity(string id) => _affinity.TryGetValue(id, out var v) ? v : 0;
        public void AddAffinity(string id, int delta)
            => _affinity[id] = Mathf.Max(0, Affinity(id) + delta);

        // Lv.3 / Lv.5 해금 효과는 표(문서 05-3)대로 각 시설/상점에서 조회.
        public int AffinityLevel(string id)
        {
            int a = Affinity(id);
            return a >= 100 ? 5 : a >= 40 ? 3 : a >= 10 ? 1 : 0;
        }
    }
}
