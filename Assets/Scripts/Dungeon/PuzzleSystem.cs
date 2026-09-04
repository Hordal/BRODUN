// PuzzleSystem.cs — 퍼즐 런타임 베이스 (문서 03-4 / 06-6 힌트·스킵 접근성).
using UnityEngine;
using BroDungeon.Characters;
using BroDungeon.Utilities;

namespace BroDungeon.Dungeon
{
    public abstract class PuzzleSystem : MonoBehaviour
    {
        public string puzzleId = "P01";
        public bool Solved { get; protected set; }

        [Header("접근성 (문서 06-6)")]
        public float hintDelay = 30f;   // 30초 후 힌트 노출
        public int skipAfterFails = 3;  // 이야기 모드 3회 실패 시 스킵

        float _elapsed;
        int _fails;
        bool _hintShown;

        public PuzzleDef Def { get; private set; }

        protected virtual void Awake() { Def = PuzzleTable.Get(puzzleId); }

        protected virtual void Update()
        {
            if (Solved) return;
            _elapsed += Time.deltaTime;
            if (!_hintShown && _elapsed >= hintDelay) { _hintShown = true; ShowHint(); }
        }

        /// 서브클래스가 정답 조건 충족 시 호출.
        protected void Solve()
        {
            if (Solved) return;
            Solved = true;
            BondSystem.Instance?.Add(Data.Constants.BOND_ON_COMBO); // 협동 보상
            OnSolved();
        }

        public void RegisterFail()
        {
            _fails++;
            if (CanSkip() && _fails >= skipAfterFails) Solve(); // 이야기 모드 자동 스킵
        }

        bool CanSkip() => Core.GameManager.Instance != null
                          && Core.GameManager.Instance.Difficulty == Data.Difficulty.Story;

        protected virtual void ShowHint() => Debug.Log($"[Puzzle {puzzleId}] 힌트: {Def.Solution}");
        protected abstract void OnSolved();
    }

    /// 간단한 레버/스위치 기반 퍼즐 공통 구현 예시.
    public class SwitchPuzzle : PuzzleSystem
    {
        public int requiredSwitches = 2;
        int _active;

        public void ToggleSwitch(bool on)
        {
            _active += on ? 1 : -1;
            if (_active >= requiredSwitches) Solve();
        }

        protected override void OnSolved() => Debug.Log($"[Puzzle {puzzleId}] 해결!");
    }
}
