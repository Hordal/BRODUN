// CraftingSystem.cs — 연금 레시피 10종 + 강화 + 분해 + 음식 8종 (문서 05-1)
using System.Collections.Generic;
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Items
{
    public class Recipe
    {
        public string Name, Result;
        public Dictionary<string, int> Materials;
        public Recipe(string name, string result, Dictionary<string, int> mats)
        { Name = name; Result = result; Materials = mats; }
    }

    public class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); Build(); }

        public readonly List<Recipe> Alchemy = new List<Recipe>();
        public readonly List<Recipe> Food = new List<Recipe>();
        public float materialDiscount = 0f; // 유물 R15/연금술사의 솥

        static Dictionary<string,int> Mat(params (string,int)[] xs)
        { var d=new Dictionary<string,int>(); foreach(var x in xs) d[x.Item1]=x.Item2; return d; }

        void Build()
        {
            // 연금 레시피 10종 (문서 05-1)
            Alchemy.Add(new Recipe("지속 회복 물약","10초 HP 지속 회복", Mat(("체력물약",1),("재생초",2))));
            Alchemy.Add(new Recipe("마나 폭발 물약","마나 전회복", Mat(("마나물약",1),("마력수정",3))));
            Alchemy.Add(new Recipe("해독제","독/저주 해제", Mat(("독버섯",2),("물",1))));
            Alchemy.Add(new Recipe("독 도포제","무기 독 부여(3분)", Mat(("독버섯",3),("점착액",1))));
            Alchemy.Add(new Recipe("빙결 폭탄","투척 범위 빙결", Mat(("빙결수정",2),("폭약",1))));
            Alchemy.Add(new Recipe("화염 부적","화상 면역(1런)", Mat(("화염비늘",3),("가죽",1))));
            Alchemy.Add(new Recipe("번개 충전석","B 쿨 1회 초기화", Mat(("전기수정",2),("금속파편",2))));
            Alchemy.Add(new Recipe("빛의 향로","어둠 면역(5분)", Mat(("빛열매",2),("은가루",1))));
            Alchemy.Add(new Recipe("쿨 초기화 물약","전 쿨 즉시 초기화", Mat(("시간초",3),("마력수정",5))));
            Alchemy.Add(new Recipe("귀환석","거점 즉시 귀환", Mat(("마력수정",5),("룬파편",3))));

            // 음식 8종 (문서 05-1, 런당 1개)
            Food.Add(new Recipe("석화버섯 스튜","방어+8%, 넉백 저항", Mat(("석화버섯",3),("물",1))));
            Food.Add(new Recipe("빙결과일 주스","마나 회복+15%", Mat(("빙결열매",3),("물",1))));
            Food.Add(new Recipe("화염고기 구이","공격+6%, 화상 저항", Mat(("화염고기",2),("향신료",1))));
            Food.Add(new Recipe("독버섯 해독차","독/저주 저항, HP+10%", Mat(("독버섯",2),("재생초",2))));
            Food.Add(new Recipe("빛열매 파이","시야 확대, 비밀방+10%", Mat(("빛열매",3),("밀가루",1))));
            Food.Add(new Recipe("그림자 젤리","은신+2초, 배후+10%", Mat(("그림자이끼",3),("설탕수정",1))));
            Food.Add(new Recipe("시간초 수프","전 쿨 -8%", Mat(("시간초",2),("물",2))));
            Food.Add(new Recipe("미궁의 만찬","전 스탯 +5%", Mat(("1구역재료",1),("2구역재료",1),("3구역재료",1),("4구역재료",1),("5구역재료",1))));
        }

        public bool HasMaterials(Recipe r)
        {
            foreach (var kv in r.Materials)
            {
                int need = Mathf.Max(1, Mathf.RoundToInt(kv.Value * (1f - materialDiscount)));
                if (GetMaterial(kv.Key) < need) return false;
            }
            return true;
        }

        public bool Craft(Recipe r)
        {
            if (!HasMaterials(r)) return false;
            foreach (var kv in r.Materials)
            {
                int need = Mathf.Max(1, Mathf.RoundToInt(kv.Value * (1f - materialDiscount)));
                AddMaterial(kv.Key, -need);
            }
            return true;
        }

        // ── 강화 (문서 05-1: +1 100% ~ +5 50%, 실패 시 파괴 없음) ──
        public bool Enhance(InventoryEntry entry)
        {
            if (entry.Enhancement >= 5) return false;
            if (GetMaterial("강화석") <= 0) return false; // 강화석 없으면 불가
            float chance = Constants.FORGE_SUCCESS[entry.Enhancement];
            // 재료 소모는 항상, 성공 시에만 +1
            AddMaterial("강화석", -1);
            if (Random.value <= chance) { entry.Enhancement++; return true; }
            return false; // 실패: 재료만 소모(파괴 없음)
        }

        // ── 분해 (문서 05-1) ──
        public void Disassemble(InventoryEntry entry)
        {
            var def = Database.ItemTable.Get(entry.ItemId);
            if (def == null) return;
            switch (def.Grade)
            {
                case Grade.Common: AddMaterial("강화석", 1); break;
                case Grade.Rare: AddMaterial("강화석", 3); AddMaterial("상급강화석", 1); break;
                case Grade.Legendary: AddMaterial("상급강화석", 3); AddMaterial("전설파편", 1); break;
            }
            InventorySystem.Instance?.Remove(entry);
        }

        // 재료 저장은 StorageData.materials에 위임
        Data.StorageData Store => Core.SaveManager.Instance != null ? Core.SaveManager.Instance.Data.storage : _local;
        readonly Data.StorageData _local = new Data.StorageData();
        int GetMaterial(string id) => Store.materials.TryGetValue(id, out var v) ? v : 0;
        void AddMaterial(string id, int n) => Store.materials[id] = Mathf.Max(0, GetMaterial(id) + n);
    }
}
