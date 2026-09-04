// Constants.cs — GDD 전역 상수 (수치 TBD는 주석 표기)
namespace BroDungeon.Data
{
    public static class Constants
    {
        // ── 캐릭터 기본 수치 (문서 01-2) ──
        public const float A_BASE_HP = 500f;
        public const float A_BASE_ATK = 50f;
        public const float A_BASE_DEF = 20f;
        public const float A_BASE_STAMINA = 100f;

        public const float B_BASE_HP = 200f;
        public const float B_BASE_ATK = 30f;   // 마법
        public const float B_BASE_DEF = 5f;
        public const float B_BASE_MANA = 200f;

        // ── HP/다운/리바이브 (문서 01-3, I-06) — [TBD: 팀 확정] ──
        public const float B_DOWN_DURATION = 8f;     // 다운 카운트다운(초)
        public const float B_REVIVE_HP_RATIO = 0.30f; // A 회수 시 부활 HP%
        public const float A_SOLO_DEBUFF = 0.20f;     // B 사망 시 A 공격 -20%

        // ── 분리 디버프 (문서 01-3) ──
        public const float SEPARATION_MAX_DISTANCE = 12f; // 이 거리 초과 시 디버프
        public const float MERGE_CONTACT_RADIUS = 1.2f;   // 접촉 시 자동 합체

        // ── 데미지 공식 (문서 01-5 / 06-4) ──
        public const float DEFENSE_K = 100f; // 방어율 = DEF / (DEF + K)

        // 상태이상 DoT 스택 상한(반복 피격 시 무한 누적 방지)
        public const int STATUS_MAX_STACKS = 5;

        // ── 유대 시스템 (문서 01-6) ──
        public static readonly int[] BOND_TIER_MIN = { 0, 50, 150, 300, 500 };
        public static readonly float[] BOND_STAT_BONUS = { 0f, 0.03f, 0.05f, 0.08f, 0.12f };
        public static readonly float[] BOND_SEP_REDUCE = { 0f, 0f, 0.20f, 0.40f, 0.60f };
        public static readonly float[] BOND_COMBO_BONUS = { 0f, 0f, 0f, 0.10f, 0.20f };
        public const int BOND_ON_PARRY = 1, BOND_ON_COMBO = 1, BOND_ON_PROTECT = 1;
        public const int BOND_ON_B_HIT = -2, BOND_ON_B_DEATH = -10;

        // 합체 전용 스킬 쿨다운(초)
        public const float COMBO_SMASH_CD = 20f;   // 합체 강타 (3등급)
        public const float BOND_BARRIER_CD = 45f;  // 유대의 방벽 (4등급)
        public const float ULTIMATE_CD = 120f;     // 두 사람의 일격 (5등급)

        // ── 시너지 (문서 02-1, I-05) ──
        public const int SYNERGY_MAX_ITEM = 3;
        public const int SYNERGY_MAX_SKILL = 3;
        public const int SYNERGY_MAX_RUNE = 2; // [TBD: 가중치 1 vs 0.5]
        public const float SYNERGY_RUNE_WEIGHT = 1f;
        public const float SYNERGY_2SET = 0.20f; // 소형: 해당 효과 20%↑

        // ── 룬 정제 배율 (문서 02-2) ──
        public const float RUNE_REFINE_NORMAL = 1.0f;
        public const float RUNE_REFINE_REFINED = 1.5f;
        public const float RUNE_REFINE_PURE = 2.0f;

        // ── 던전 구조 (문서 03-1) ──
        public const int ROOMS_PER_FLOOR_MIN = 8;
        public const int ROOMS_PER_FLOOR_MAX = 12;
        public static readonly int[] FLOORS_PER_ZONE = { 3, 3, 3, 2, 2 }; // 총 13층
        public const int ZONE_COUNT = 5;

        // 방 유형 가중치(%) (문서 03-1): Combat/Puzzle/Trap/Event/Boss
        public static readonly int[] ROOM_WEIGHT = { 40, 25, 15, 15, 5 };

        // ── 제물 경제 (문서 05-2, I-04 안B) — [TBD] ──
        public const int RUN_SACRIFICE_GAIN = 1400;
        public const int ALTAR_APPEAR_MIN = 4, ALTAR_APPEAR_MAX = 6;

        // ── 강화 (문서 05-1) ──
        public static readonly float[] FORGE_SUCCESS = { 1.0f, 0.9f, 0.8f, 0.65f, 0.5f }; // +1~+5

        // ── 엔드리스 스케일링 (문서 05-4) ──
        public static float EndlessHpMul(int floor)
        {
            if (floor <= 25) return 1.0f;
            if (floor <= 50) return 1.5f;
            if (floor <= 75) return 2.0f;
            if (floor <= 100) return 2.5f;
            return 3.0f + 0.1f * (floor - 100);
        }
        public static float EndlessAtkMul(int floor)
        {
            if (floor <= 25) return 1.0f;
            if (floor <= 50) return 1.3f;
            if (floor <= 75) return 1.6f;
            if (floor <= 100) return 2.0f;
            return 2.5f + 0.05f * (floor - 100);
        }

        // ── 아트 (문서 06-2) ──
        public const int PIXELS_PER_UNIT = 16;
        public const int NATIVE_WIDTH = 480, NATIVE_HEIGHT = 270;

        // ── 세이브 ──
        public const string SAVE_FOLDER = "BroDungeon"; // [TBD: 게임명 확정 시 변경 — I-08]
        public const int SAVE_VERSION = 1;
    }
}
