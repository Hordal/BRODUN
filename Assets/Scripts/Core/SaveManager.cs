// SaveManager.cs — JSON 세이브/로드 (문서 06-5, 06-7). 자동/거점/중단 3유형.
using System.IO;
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public SaveData Data { get; private set; } = new SaveData();

        string Dir => Path.Combine(Application.persistentDataPath, Constants.SAVE_FOLDER);
        string PermanentPath => Path.Combine(Dir, "permanent.json"); // 영구
        string StoragePath => Path.Combine(Dir, "storage.json");     // 보관함/재료/룬(영구)
        string RunPath => Path.Combine(Dir, "run.json");             // 런(1회용)

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Directory.CreateDirectory(Dir);
            LoadPermanent();
        }

        // ── 영구(거점) 저장 — permanent + storage 함께 ──
        public void SavePermanent()
        {
            File.WriteAllText(PermanentPath, JsonUtility.ToJson(Data.permanent, true));
            File.WriteAllText(StoragePath, JsonUtility.ToJson(Data.storage, true));
        }

        public void LoadPermanent()
        {
            if (File.Exists(PermanentPath))
                Data.permanent = JsonUtility.FromJson<PermanentData>(File.ReadAllText(PermanentPath))
                                 ?? new PermanentData();
            if (File.Exists(StoragePath))
                Data.storage = JsonUtility.FromJson<StorageData>(File.ReadAllText(StoragePath))
                               ?? new StorageData();
        }

        // ── 런(자동/중단) 저장. 로드 시 삭제 → 세이브 스컴 방지(문서 06-7) ──
        public void SaveRun()
        {
            Data.run.active = true;
            File.WriteAllText(RunPath, JsonUtility.ToJson(Data.run, true));
        }

        public bool HasRunSave() => File.Exists(RunPath);

        public RunData LoadRunAndConsume()
        {
            if (!File.Exists(RunPath)) return null;
            var run = JsonUtility.FromJson<RunData>(File.ReadAllText(RunPath));
            File.Delete(RunPath); // 1회용: 로드 즉시 삭제
            Data.run = run ?? new RunData();
            return Data.run;
        }

        public void DeleteRun()
        {
            if (File.Exists(RunPath)) File.Delete(RunPath);
            Data.run = new RunData();
        }
    }
}
