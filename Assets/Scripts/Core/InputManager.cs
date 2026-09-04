// InputManager.cs — 입력 → 캐릭터/시스템 라우팅 (문서 01-3,4 / 06-6 리바인딩)
// 싱글: 1P가 A/B 모두, 쉬프트로 전환. 코옵: 1P=A, 2P=B(분리 시 각자).
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Characters;
using BroDungeon.Combat;

namespace BroDungeon.Core
{
    public class InputManager : MonoBehaviour
    {
        public WarriorController warrior;
        public MageController mage;
        public MergeSystem merge;
        public ThrowSystem throwSystem;
        public ParrySystem parry;
        public SkillSystem warriorSkills;
        public SkillSystem mageSkills;
        public Camera cam;

        [Header("접근성 (문서 06-6)")]
        public bool oneButtonAttack;
        public bool autoAim;
        public float inputDelay; // ±딜레이

        // 싱글에서 분리 시 조작 대상(쉬프트 토글)
        bool _controllingB;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.GameOver) return;
            if (merge == null) return; // 와이어링 미완 씬에서 매 프레임 NRE 방지
            HandleMerge();
            HandleThrowCharge();

            if (merge.IsMerged) HandleMerged();
            else HandleSeparated();
        }

        void HandleMerge()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (merge.State == MergeState.Merged) parry?.OnSeparateInput(); // 분리 직전 패리 윈도우
                merge.ToggleMerge();
                _controllingB = false;
            }
            // 싱글 분리 중 A↔B 조작 전환
            if (!merge.IsMerged && GameManager.Instance != null
                && GameManager.Instance.NetMode == NetMode.Single
                && Input.GetKeyDown(KeyCode.Tab))
                _controllingB = !_controllingB;
        }

        // ── 합체 조작: A 주도, B 자동/수동 보조 ──
        void HandleMerged()
        {
            float ax = Input.GetAxisRaw("Horizontal");
            warrior.Move(ax);
            if (Input.GetButtonDown("Jump")) warrior.Jump();
            if (Input.GetKeyDown(KeyCode.LeftControl)) warrior.Dash();
            if (Input.GetMouseButtonDown(0)) warrior.Attack();
            warrior.SetBlock(Input.GetMouseButton(1));

            // 합체 시 B 보조: 싱글=자동, 코옵=2P 수동(문서 01-4)
            if (Input.GetKeyDown(KeyCode.Q)) mageSkills.CastSlot(0, AimWorld());
            if (Input.GetKeyDown(KeyCode.E)) mageSkills.CastSlot(1, AimWorld());

            // A 스킬
            if (Input.GetKeyDown(KeyCode.Alpha1)) warriorSkills.CastSlot(0, AimWorld());
            if (Input.GetKeyDown(KeyCode.Alpha2)) warriorSkills.CastSlot(1, AimWorld());
        }

        // ── 분리 조작 ──
        void HandleSeparated()
        {
            bool single = GameManager.Instance == null || GameManager.Instance.NetMode == NetMode.Single;
            float ax = Input.GetAxisRaw("Horizontal");

            // B가 사망/부재면 B 조작 불가 → A 조작으로 강제 복귀
            if (_controllingB && mage != null && mage.State == DownState.Dead) _controllingB = false;

            if (single)
            {
                if (_controllingB) { mage.Crawl(ax); CastBInput(); }
                else { warrior.Move(ax); if (Input.GetButtonDown("Jump")) warrior.Jump(); CastAInput(); }
            }
            else
            {
                // 코옵: A=1P 입력축, B=2P 입력축 (Netcode가 분리 라우팅)
                warrior.Move(ax);
                CastAInput(); CastBInput();
            }
        }

        void CastAInput()
        {
            if (Input.GetMouseButtonDown(0)) warrior.Attack();
            if (Input.GetKeyDown(KeyCode.Alpha1)) warriorSkills.CastSlot(0, AimWorld());
            if (Input.GetKeyDown(KeyCode.Alpha2)) warriorSkills.CastSlot(1, AimWorld());
            if (Input.GetKeyDown(KeyCode.LeftControl)) warrior.Dash();
        }

        void CastBInput()
        {
            if (Input.GetKeyDown(KeyCode.Q)) mageSkills.CastSlot(0, AimWorld());
            if (Input.GetKeyDown(KeyCode.E)) mageSkills.CastSlot(1, AimWorld());
        }

        // 던지기: 쉬프트 차지 → 떼면 발사(문서 01-3)
        void HandleThrowCharge()
        {
            if (merge.State != MergeState.Separated) return;
            if (Input.GetKeyDown(KeyCode.G)) throwSystem.BeginCharge();
            if (Input.GetKey(KeyCode.G)) throwSystem.TickCharge(Time.deltaTime);
            if (Input.GetKeyUp(KeyCode.G)) throwSystem.Release(AimWorld());
        }

        Vector2 AimWorld()
        {
            if (autoAim) return AutoAimTarget();
            return cam ? (Vector2)cam.ScreenToWorldPoint(Input.mousePosition) : Vector2.right;
        }

        Vector2 AutoAimTarget()
        {
            // 가장 가까운 적 자동 조준(접근성). 구현 시 적 매니저 조회.
            return (Vector2)warrior.transform.position + Vector2.right * warrior.transform.localScale.x;
        }
    }
}
