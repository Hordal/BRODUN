// DataAssetGenerator.cs — 정적 시드 테이블(Database/*) → ScriptableObject .asset 자동 생성.
// 메뉴: BroDungeon ▸ Generate Data Assets. 생성 위치: Assets/Resources/Data/...
// 기존 에셋이 있으면 값만 갱신(GUID 유지 → 참조 보존).
using System.IO;
using UnityEditor;
using UnityEngine;
using BroDungeon.Data.SO;
using BroDungeon.Database;

namespace BroDungeon.EditorTools
{
    public static class DataAssetGenerator
    {
        const string Root = "Assets/Resources/Data";

        [MenuItem("BroDungeon/Generate Data Assets")]
        public static void GenerateAll()
        {
            EnsureFolders();
            int n = 0;
            n += GenerateItems();
            n += GenerateSkills();
            n += GenerateMonsters();
            n += GenerateBosses();
            n += GenerateRelics();
            n += GenerateGadgets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[DataAssetGenerator] 완료: {n}개 ScriptableObject 에셋 생성/갱신 → {Root}");
        }

        [MenuItem("BroDungeon/Clear Generated Data Assets")]
        public static void ClearAll()
        {
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
                AssetDatabase.Refresh();
                Debug.Log("[DataAssetGenerator] 생성된 데이터 에셋 삭제 완료");
            }
        }

        static void EnsureFolders()
        {
            CreateFolder("Assets/Resources");
            CreateFolder(Root);
            foreach (var sub in new[] { "Items", "Skills", "Monsters", "Bosses", "Relics", "Gadgets" })
                CreateFolder($"{Root}/{sub}");
        }

        static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        /// 기존 에셋 재사용(GUID 보존) 또는 신규 생성.
        static T GetOrCreate<T>(string folder, string id) where T : ScriptableObject
        {
            string path = $"{folder}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<T>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(so, path);
            }
            return so;
        }

        static int GenerateItems()
        {
            int c = 0;
            foreach (var def in ItemTable.All.Values)
            {
                var so = GetOrCreate<ItemSO>($"{Root}/Items", def.Id);
                so.id = def.Id; so.displayName = def.Name;
                so.baseEffect = def.BaseEffect; so.special = def.Special; so.curse = def.Curse;
                so.category = def.Category; so.slot = def.Slot;
                so.attr1 = def.Attr1; so.attr2 = def.Attr2; so.grade = def.Grade;
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }

        static int GenerateSkills()
        {
            int c = 0;
            foreach (var def in SkillTable.All.Values)
            {
                var so = GetOrCreate<SkillSO>($"{Root}/Skills", def.Id);
                so.id = def.Id; so.displayName = def.Name; so.desc = def.Desc;
                so.attribute = def.Attribute; so.owner = def.Owner; so.type = def.Type;
                so.cooldown = def.Cooldown; so.manaCost = def.ManaCost; so.damageMul = def.DamageMul;
                so.healAmount = def.HealAmount; so.duration = def.Duration; so.magnitude = def.Magnitude;
                so.areaRadius = def.AreaRadius; so.chainCount = def.ChainCount; so.bounce = def.Bounce;
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }

        static int GenerateMonsters()
        {
            int c = 0;
            foreach (var def in MonsterTable.All.Values)
            {
                var so = GetOrCreate<MonsterSO>($"{Root}/Monsters", def.Id);
                so.id = def.Id; so.displayName = def.Name; so.behavior = def.Behavior; so.counter = def.Counter;
                so.zone = def.Zone; so.attr1 = def.Attr1; so.attr2 = def.Attr2; so.type = def.Type;
                so.maxHp = MonsterTable.ResolveHp(def.Zone, def.Hp);
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }

        static int GenerateBosses()
        {
            int c = 0;
            foreach (var def in BossTable.All.Values)
            {
                var so = GetOrCreate<BossSO>($"{Root}/Bosses", def.Id);
                so.id = def.Id; so.displayName = def.Name; so.appearance = def.Appearance;
                so.reward = def.Reward; so.zone = def.Zone; so.attrs = def.Attrs;
                so.maxHp = def.MaxHp; so.phasePatterns = def.PhasePatterns;
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }

        static int GenerateRelics()
        {
            int c = 0;
            foreach (var def in RelicTable.All.Values)
            {
                var so = GetOrCreate<RelicSO>($"{Root}/Relics", def.Id);
                so.id = def.Id; so.displayName = def.Name; so.effect = def.Effect; so.grade = def.Grade;
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }

        static int GenerateGadgets()
        {
            int c = 0;
            foreach (var def in GadgetTable.All.Values)
            {
                var so = GetOrCreate<GadgetSO>($"{Root}/Gadgets", def.Id);
                so.id = def.Id; so.displayName = def.Name;
                so.mergedEffect = def.MergedEffect; so.separatedB = def.SeparatedB; so.separatedA = def.SeparatedA;
                EditorUtility.SetDirty(so); c++;
            }
            return c;
        }
    }
}
