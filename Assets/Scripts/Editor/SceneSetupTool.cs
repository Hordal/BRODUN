// SceneSetupTool.cs — 슬라이스/Boot 씬 자동 생성 (수동 GameObject 배치 불필요).
// 메뉴: BroDungeon ▸ Create Slice Scene / Create Boot Scene.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using BroDungeon.Slice;

namespace BroDungeon.EditorTools
{
    public static class SceneSetupTool
    {
        const string SceneDir = "Assets/Scenes";

        [MenuItem("BroDungeon/Create Slice Scene")]
        public static void CreateSliceScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 기본 카메라는 SliceBootstrap이 직접 구성하므로 남겨둠.
            var go = new GameObject("~SliceBootstrap");
            go.AddComponent<SliceBootstrap>();

            EnsureDir();
            string path = $"{SceneDir}/Slice.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[SceneSetup] 슬라이스 씬 생성: {path}  ▶ Play 누르면 1구역 전투 슬라이스 실행");
        }

        [MenuItem("BroDungeon/Create Boot Scene")]
        public static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var managers = new GameObject("~Bootstrap");
            managers.AddComponent<BroDungeon.Core.SaveManager>();
            managers.AddComponent<BroDungeon.Core.GameManager>();
            managers.AddComponent<BroDungeon.Core.AudioManager>();
            managers.AddComponent<BroDungeon.Network.NetworkManager>();
            managers.AddComponent<BroDungeon.Core.GameBootstrap>();

            EnsureDir();
            string path = $"{SceneDir}/Boot.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[SceneSetup] Boot 씬 생성: {path} (GameBootstrap 참조는 인스펙터에서 연결)");
        }

        static void EnsureDir()
        {
            if (!AssetDatabase.IsValidFolder(SceneDir))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
