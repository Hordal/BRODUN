// BossPattern.cs — 보스 페이즈별 액션 정의 + 보스별 구성 라이브러리 (문서 04-2).
// 제네릭 액션을 조합해 5보스 + 히든의 페이즈 패턴을 데이터로 표현.
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.AI
{
    public enum BossActionType
    {
        MeleeArc,         // 근접 호 범위
        AreaSlam,         // 자기중심 광역
        ProjectileSpread, // 부채꼴 다발
        ProjectileAimed,  // 조준 단발/연발
        Beam,             // 직선 빔(근사: 전방 긴 범위)
        ShieldUp,         // 보호막(무적/경감)
        Charge,           // 돌진
        Summon            // 소환
    }

    public struct BossAction
    {
        public BossActionType Type;
        public float Telegraph;  // 예고 시간(초)
        public float Recovery;   // 후딜(초)
        public float Damage;
        public float Radius;
        public int Count;        // 투사체/소환 수
        public float Param;      // 보호막 지속/돌진 속도 등
        public AttributeType Attr;
        public string SummonId;

        public static BossAction Make(BossActionType t, float tel, float rec, float dmg,
            float radius = 2.5f, int count = 1, float param = 0f,
            AttributeType attr = AttributeType.None, string summon = null)
            => new BossAction { Type = t, Telegraph = tel, Recovery = rec, Damage = dmg,
                Radius = radius, Count = count, Param = param, Attr = attr, SummonId = summon };
    }

    public static class BossPatternLibrary
    {
        /// 보스 id → 페이즈별 액션 리스트. 페이즈 수는 BossTable 패턴 수와 일치.
        public static List<BossAction>[] Build(string bossId, float atkBase)
        {
            switch (bossId)
            {
                case "BOSS1": // 석화의 왕 고르간 (땅+바위) 3페이즈
                    return new[]
                    {
                        Phase( // P1: 대검3연→충격파→방패 무적
                            A(BossActionType.MeleeArc, 0.7f, 0.4f, atkBase, 3.0f),
                            A(BossActionType.AreaSlam, 1.0f, 0.8f, atkBase * 1.3f, 4.0f, attr: AttributeType.Earth),
                            A(BossActionType.ShieldUp, 0.3f, 5.0f, 0, param: 5f)),
                        Phase( // P2: 4팔 + 바닥진동 + B추적
                            A(BossActionType.MeleeArc, 0.5f, 0.3f, atkBase, 3.5f),
                            A(BossActionType.AreaSlam, 0.9f, 0.6f, atkBase * 1.2f, 4.5f, attr: AttributeType.Rock)),
                        Phase( // P3: 바위 회전 + 낙석 + 약점
                            A(BossActionType.ProjectileSpread, 0.8f, 0.5f, atkBase, count: 6, attr: AttributeType.Rock),
                            A(BossActionType.Charge, 0.6f, 0.7f, atkBase * 1.4f, param: 14f)),
                    };

                case "BOSS2": // 심연의 여왕 나이아 (물+얼음+독) 3페이즈
                    return new[]
                    {
                        Phase(
                            A(BossActionType.ProjectileSpread, 0.7f, 0.4f, atkBase, count: 3, attr: AttributeType.Water),
                            A(BossActionType.Beam, 1.0f, 0.6f, atkBase * 1.2f, 5f),
                            A(BossActionType.AreaSlam, 0.9f, 0.7f, atkBase, 3.5f, attr: AttributeType.Poison)),
                        Phase(
                            A(BossActionType.AreaSlam, 1.2f, 1.0f, atkBase * 1.3f, 6f, attr: AttributeType.Ice),
                            A(BossActionType.ProjectileAimed, 0.5f, 0.3f, atkBase, count: 2, attr: AttributeType.Water)),
                        Phase(
                            A(BossActionType.ProjectileSpread, 0.6f, 0.4f, atkBase, count: 6, attr: AttributeType.Poison),
                            A(BossActionType.Beam, 0.8f, 0.5f, atkBase * 1.3f, 6f, attr: AttributeType.Ice)),
                    };

                case "BOSS3": // 화로의 심판관 이그나투스 (불+번개+폭발) 3페이즈
                    return new[]
                    {
                        Phase(
                            A(BossActionType.MeleeArc, 0.6f, 0.4f, atkBase, 3.5f, attr: AttributeType.Fire),
                            A(BossActionType.Beam, 1.0f, 0.6f, atkBase * 1.3f, 6f, attr: AttributeType.Fire)),
                        Phase(
                            A(BossActionType.AreaSlam, 0.9f, 0.6f, atkBase * 1.2f, 5f, attr: AttributeType.Lightning),
                            A(BossActionType.ProjectileSpread, 0.8f, 0.5f, atkBase, count: 5, attr: AttributeType.Explosion)),
                        Phase( // 코어 노출 + 용암/낙뢰
                            A(BossActionType.AreaSlam, 0.7f, 0.5f, atkBase * 1.4f, 6f, attr: AttributeType.Explosion),
                            A(BossActionType.ProjectileAimed, 0.4f, 0.2f, atkBase, count: 3, attr: AttributeType.Lightning)),
                    };

                case "BOSS4": // 쌍면의 재판관 유디스 (빛+어둠) 3페이즈
                    return new[]
                    {
                        Phase(
                            A(BossActionType.MeleeArc, 0.5f, 0.4f, atkBase, 3.5f, attr: AttributeType.Light),
                            A(BossActionType.MeleeArc, 0.5f, 0.4f, atkBase, 3.5f, attr: AttributeType.Dark)),
                        Phase(
                            A(BossActionType.Summon, 1.0f, 1.5f, 0, count: 2, summon: "M29"),
                            A(BossActionType.Beam, 0.9f, 0.6f, atkBase * 1.3f, 6f, attr: AttributeType.Dark)),
                        Phase(
                            A(BossActionType.ProjectileSpread, 0.7f, 0.4f, atkBase, count: 8, attr: AttributeType.Light),
                            A(BossActionType.AreaSlam, 0.9f, 0.7f, atkBase * 1.4f, 6f, attr: AttributeType.Dark)),
                    };

                case "BOSS5": // 심연의 관리자 에테르나 (시간+중력+연쇄) 4페이즈
                    return new[]
                    {
                        Phase(
                            A(BossActionType.Beam, 0.9f, 0.5f, atkBase, 7f, attr: AttributeType.Light),
                            A(BossActionType.Beam, 0.9f, 0.5f, atkBase, 7f, attr: AttributeType.Dark)),
                        Phase(
                            A(BossActionType.Summon, 1.0f, 1.5f, 0, count: 2, summon: "M40"),
                            A(BossActionType.ProjectileSpread, 0.7f, 0.4f, atkBase, count: 6, attr: AttributeType.Gravity)),
                        Phase(
                            A(BossActionType.AreaSlam, 0.8f, 0.6f, atkBase * 1.3f, 6f, attr: AttributeType.Time),
                            A(BossActionType.ProjectileAimed, 0.4f, 0.2f, atkBase, count: 4, attr: AttributeType.Chain)),
                        Phase( // 12룬 전속성 난사
                            A(BossActionType.ProjectileSpread, 0.6f, 0.3f, atkBase * 1.2f, count: 12, attr: AttributeType.None)),
                    };

                case "BOSSH": // 히든: 설계자 아키텍트 3페이즈(보스 패턴 순환)
                    return new[]
                    {
                        Phase(
                            A(BossActionType.MeleeArc, 0.5f, 0.3f, atkBase, 4f),
                            A(BossActionType.ProjectileSpread, 0.6f, 0.3f, atkBase, count: 8),
                            A(BossActionType.Beam, 0.8f, 0.4f, atkBase * 1.3f, 7f)),
                        Phase(
                            A(BossActionType.AreaSlam, 0.7f, 0.4f, atkBase * 1.3f, 7f, attr: AttributeType.Time),
                            A(BossActionType.Summon, 1.0f, 1.0f, 0, count: 3, summon: "M44")),
                        Phase(
                            A(BossActionType.ProjectileSpread, 0.5f, 0.2f, atkBase * 1.4f, count: 16)),
                    };

                default:
                    return new[] { Phase(A(BossActionType.MeleeArc, 0.7f, 0.5f, atkBase, 3f)) };
            }
        }

        static List<BossAction> Phase(params BossAction[] actions) => new List<BossAction>(actions);
        static BossAction A(BossActionType t, float tel, float rec, float dmg,
            float radius = 2.5f, int count = 1, float param = 0f,
            AttributeType attr = AttributeType.None, string summon = null)
            => BossAction.Make(t, tel, rec, dmg, radius, count, param, attr, summon);
    }
}
