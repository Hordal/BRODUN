// SaveData.cs — JSON 직렬화 세이브 구조 (문서 06-5, 06-7)
// permanent + run + storage 3블록.
// ⚠ JsonUtility는 Dictionary를 직렬화하지 못한다 → 런타임은 Dictionary로 쓰되
//   ISerializationCallbackReceiver로 직렬화용 List와 동기화한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BroDungeon.Data
{
    [Serializable] public struct StrInt { public string key; public int value; }
    [Serializable] public struct StrStr { public string key; public string value; }

    static class DictSerial
    {
        public static List<StrInt> ToList(Dictionary<string, int> d)
        {
            var l = new List<StrInt>(d.Count);
            foreach (var kv in d) l.Add(new StrInt { key = kv.Key, value = kv.Value });
            return l;
        }
        public static Dictionary<string, int> ToDict(List<StrInt> l)
        {
            var d = new Dictionary<string, int>();
            if (l != null) foreach (var p in l) d[p.key] = p.value;
            return d;
        }
        public static List<StrStr> ToList(Dictionary<string, string> d)
        {
            var l = new List<StrStr>(d.Count);
            foreach (var kv in d) l.Add(new StrStr { key = kv.Key, value = kv.Value });
            return l;
        }
        public static Dictionary<string, string> ToDict(List<StrStr> l)
        {
            var d = new Dictionary<string, string>();
            if (l != null) foreach (var p in l) d[p.key] = p.value;
            return d;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version = Constants.SAVE_VERSION;
        public PermanentData permanent = new PermanentData();
        public RunData run = new RunData();
        public StorageData storage = new StorageData();
    }

    /// 영구 유지(거점 저장). 문서 06-7.
    [Serializable]
    public class PermanentData : ISerializationCallbackReceiver
    {
        public int bondPoints;                       // 유대 (문서 01-6)
        public List<string> unlockedSlots = new List<string>(); // 슬롯 해금
        public List<string> rescuedNpcs = new List<string>();
        public List<int> clearedZones = new List<int>();
        public List<string> achievements = new List<string>();
        public List<string> discoveredRunes = new List<string>(); // 룬 조합 발견
        public List<string> knownRecipes = new List<string>();
        public int difficulty = (int)Difficulty.Normal;
        public SettingsData settings = new SettingsData();
        public int labyrinthCrystal;

        [NonSerialized] public Dictionary<string, int> permUpgrades = new Dictionary<string, int>();
        [SerializeField] List<StrInt> _permUpgrades = new List<StrInt>();

        public void OnBeforeSerialize() => _permUpgrades = DictSerial.ToList(permUpgrades);
        public void OnAfterDeserialize() => permUpgrades = DictSerial.ToDict(_permUpgrades);
    }

    /// 활성 런 상태 (자동/중단 저장, 로드 시 삭제). 문서 06-7.
    [Serializable]
    public class RunData
    {
        public bool active;
        public int mode = (int)GameMode.Story;
        public int zone = 1, floor = 1, room;
        public int seed;                  // 절차 맵 동기화(멀티)
        public CharacterState a = new CharacterState();
        public CharacterState b = new CharacterState();
        public List<string> inventory = new List<string>();
        public List<string> relics = new List<string>();
        public string gadget;
        public List<string> altarBuffs = new List<string>();
        public int gold, sacrifice;
        public int weather = -1;          // 현재 층 환경 변이 (문서 05-6)
    }

    [Serializable]
    public class CharacterState
    {
        public float hp;
        public int downState;             // DownState
        public List<string> equipped = new List<string>();
        public List<string> skills = new List<string>();
        public string passive;
    }

    /// 보관함/재료/룬 (런 무관 유지). 문서 06-7.
    [Serializable]
    public class StorageData : ISerializationCallbackReceiver
    {
        public List<string> stored = new List<string>();
        public int storageSize = 20;      // 20→100 확장

        [NonSerialized] public Dictionary<string, int> materials = new Dictionary<string, int>();
        [NonSerialized] public Dictionary<string, int> runes = new Dictionary<string, int>(); // 속성→개수
        [NonSerialized] public Dictionary<string, int> currencies = new Dictionary<string, int>();
        [SerializeField] List<StrInt> _materials = new List<StrInt>();
        [SerializeField] List<StrInt> _runes = new List<StrInt>();
        [SerializeField] List<StrInt> _currencies = new List<StrInt>();

        public void OnBeforeSerialize()
        {
            _materials = DictSerial.ToList(materials);
            _runes = DictSerial.ToList(runes);
            _currencies = DictSerial.ToList(currencies);
        }
        public void OnAfterDeserialize()
        {
            materials = DictSerial.ToDict(_materials);
            runes = DictSerial.ToDict(_runes);
            currencies = DictSerial.ToDict(_currencies);
        }
    }

    [Serializable]
    public class SettingsData : ISerializationCallbackReceiver
    {
        public float bgmVolume = 0.8f, sfxVolume = 1f, voiceVolume = 1f, uiVolume = 1f;
        public int colorBlindMode;        // 문서 06-6
        public bool highContrast, enemyHighlight, subtitles = true, monoAudio;
        public float uiScale = 1f;
        public int screenShake = 1, flash = 1;
        public bool oneButtonAttack, autoAim, autoMergeReturn;
        public float inputDelay;          // ±0.1~0.5

        [NonSerialized] public Dictionary<string, string> keyRebinds = new Dictionary<string, string>();
        [SerializeField] List<StrStr> _keyRebinds = new List<StrStr>();

        public void OnBeforeSerialize() => _keyRebinds = DictSerial.ToList(keyRebinds);
        public void OnAfterDeserialize() => keyRebinds = DictSerial.ToDict(_keyRebinds);
    }
}
