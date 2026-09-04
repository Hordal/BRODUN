// GameDataSO.cs — 기획자가 에디터에서 수치 조정 가능한 ScriptableObject DB (문서 06-5).
// 정적 시드 테이블(Database/*)을 .asset으로 변환한 결과물의 런타임 형태.
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Data.SO
{
    [CreateAssetMenu(menuName = "BroDungeon/Item", fileName = "Item")]
    public class ItemSO : ScriptableObject
    {
        public string id, displayName, baseEffect, special, curse;
        public ItemCategory category;
        public EquipSlot slot;
        public AttributeType attr1 = AttributeType.None, attr2 = AttributeType.None;
        public Grade grade;
        public Sprite icon; // 아트 단계에서 연결
    }

    [CreateAssetMenu(menuName = "BroDungeon/Skill", fileName = "Skill")]
    public class SkillSO : ScriptableObject
    {
        public string id, displayName, desc;
        public AttributeType attribute = AttributeType.None;
        public SkillOwner owner;
        public SkillType type;
        public float cooldown, manaCost, damageMul = 1f, healAmount, duration, magnitude, areaRadius;
        public int chainCount, bounce;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "BroDungeon/Monster", fileName = "Monster")]
    public class MonsterSO : ScriptableObject
    {
        public string id, displayName, behavior, counter;
        public int zone;
        public AttributeType attr1 = AttributeType.None, attr2 = AttributeType.None;
        public EnemyType type;
        public float maxHp;
        public GameObject prefab; // 스프라이트/애니 단계에서 연결
    }

    [CreateAssetMenu(menuName = "BroDungeon/Boss", fileName = "Boss")]
    public class BossSO : ScriptableObject
    {
        public string id, displayName, appearance, reward;
        public int zone;
        public AttributeType[] attrs;
        public float maxHp;
        [TextArea] public string[] phasePatterns;
    }

    [CreateAssetMenu(menuName = "BroDungeon/Relic", fileName = "Relic")]
    public class RelicSO : ScriptableObject
    {
        public string id, displayName, effect;
        public Grade grade;
    }

    [CreateAssetMenu(menuName = "BroDungeon/Gadget", fileName = "Gadget")]
    public class GadgetSO : ScriptableObject
    {
        public string id, displayName, mergedEffect, separatedB, separatedA;
    }
}
