// SkillTable.cs — 72개 스킬 전수 인코딩 (문서 02-4)
// A 액티브 24(SKA) + A 패시브 12(PSA) + B 액티브 24(SKB) + B 패시브 12(PSB)
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    /// <summary>스킬 정의(데이터). 런타임 SkillSystem이 소비.</summary>
    public class SkillDef
    {
        public string Id;
        public string Name;
        public AttributeType Attribute;
        public SkillOwner Owner;
        public SkillType Type;
        public string Desc;
        public float Cooldown;     // 초
        public float ManaCost;     // B 스킬
        public float DamageMul;    // 스킬 배율 (0.5 = +50%)
        public float HealAmount;
        public float Duration;     // 버프/방어/CC 지속
        public float Magnitude;    // 둔화%/버프% 등
        public float AreaRadius;
        public int ChainCount;     // 연쇄/관통 대상 수
        public int Bounce;

        public SkillDef(string id, string name, AttributeType attr, SkillOwner owner, SkillType type,
                        float cd, string desc, float mana = 0, float dmgMul = 1f)
        {
            Id = id; Name = name; Attribute = attr; Owner = owner; Type = type;
            Cooldown = cd; Desc = desc; ManaCost = mana; DamageMul = dmgMul;
        }
    }

    public static class SkillTable
    {
        public static readonly Dictionary<string, SkillDef> All = Build();
        public static SkillDef Get(string id) => All.TryGetValue(id, out var s) ? s : null;

        static Dictionary<string, SkillDef> Build()
        {
            var d = new Dictionary<string, SkillDef>();
            void A(SkillDef s) => d[s.Id] = s;
            var AA = SkillOwner.A_Active; var AP = SkillOwner.A_Passive;
            var BA = SkillOwner.B_Active; var BP = SkillOwner.B_Passive;
            var T = AttributeType.None; // 헬퍼 단축 불가, 명시
            _ = T;

            // ── A 전용 액티브 24 (SKA) ──
            A(new SkillDef("SKA01","화염 베기",   AttributeType.Fire,     AA, SkillType.Attack, 6,  "전방 화염 참격, 화상"));
            A(new SkillDef("SKA02","화염 돌진",   AttributeType.Fire,     AA, SkillType.Dash,   8,  "불 돌진, 경로 화상"));
            A(new SkillDef("SKA03","빙결 강타",   AttributeType.Ice,      AA, SkillType.Attack, 7,  "강타+둔화, 빙결 적 추가"));
            A(new SkillDef("SKA04","빙벽 방어",   AttributeType.Ice,      AA, SkillType.Defense,12, "전방 빙벽(3초)"){ Duration=3 });
            A(new SkillDef("SKA05","뇌격 일섬",   AttributeType.Lightning,AA, SkillType.Attack, 8,  "순간이동 참격+감전"));
            A(new SkillDef("SKA06","번개 연무",   AttributeType.Lightning,AA, SkillType.Area,  10,  "주변 연쇄 감전"){ AreaRadius=3 });
            A(new SkillDef("SKA07","지진 강타",   AttributeType.Earth,    AA, SkillType.Area,  10,  "지면 충격파"){ AreaRadius=4 });
            A(new SkillDef("SKA08","대지 방벽",   AttributeType.Earth,    AA, SkillType.Defense,15, "돌벽 소환"){ Duration=5 });
            A(new SkillDef("SKA09","질풍 대쉬",   AttributeType.Wind,     AA, SkillType.Dash,   6,  "장거리 대쉬+넉백"));
            A(new SkillDef("SKA10","폭풍 참격",   AttributeType.Wind,     AA, SkillType.Attack, 5,  "바람칼 원거리"));
            A(new SkillDef("SKA11","신성 일격",   AttributeType.Light,    AA, SkillType.Attack, 8,  "언데드 2배"){ DamageMul=1f });
            A(new SkillDef("SKA12","빛의 수호",   AttributeType.Light,    AA, SkillType.Defense,15, "보호막"){ Duration=4 });
            A(new SkillDef("SKA13","암흑 일격",   AttributeType.Dark,     AA, SkillType.Attack, 7,  "그림자 참격+흡혈 15%"));
            A(new SkillDef("SKA14","그림자 걸음", AttributeType.Dark,     AA, SkillType.Move,  10,  "순간이동+은신(2초)"){ Duration=2 });
            A(new SkillDef("SKA15","독무 참격",   AttributeType.Poison,   AA, SkillType.Attack, 6,  "독 참격, 3초 DoT"));
            A(new SkillDef("SKA16","맹독 폭발",   AttributeType.Poison,   AA, SkillType.Area,  12,  "독 중첩 적 폭발"){ AreaRadius=3 });
            A(new SkillDef("SKA17","바위 분쇄",   AttributeType.Rock,     AA, SkillType.Attack, 8,  "강공+넉다운"));
            A(new SkillDef("SKA18","파쇄 돌진",   AttributeType.Rock,     AA, SkillType.Dash,  10,  "돌진+넉다운+방어↓"));
            A(new SkillDef("SKA19","폭렬 강타",   AttributeType.Explosion,AA, SkillType.Area,  12,  "자기 중심 폭발(자가 10%)"){ AreaRadius=3.5f });
            A(new SkillDef("SKA20","관통 찌르기", AttributeType.Pierce,   AA, SkillType.Attack, 8,  "일직선, 방어 무시 50%"));
            A(new SkillDef("SKA21","가속 연타",   AttributeType.Haste,    AA, SkillType.Buff,  15,  "3초 공속 2배"){ Duration=3, Magnitude=1f });
            A(new SkillDef("SKA22","중력 슬램",   AttributeType.Gravity,  AA, SkillType.Area,  12,  "점프 착지+끌어당기기"){ AreaRadius=4 });
            A(new SkillDef("SKA23","시간 역행",   AttributeType.Time,     AA, SkillType.Util,  30,  "3초 전 위치/HP 복귀"));
            A(new SkillDef("SKA24","연쇄 타격",   AttributeType.Chain,    AA, SkillType.Attack,10,  "최대 5체 바운스"){ ChainCount=5, Bounce=4 });

            // ── A 전용 패시브 12 (PSA) ──
            A(new SkillDef("PSA01","불굴의 의지", AttributeType.Endure, AP, SkillType.Passive,0,"HP20%↓ 방어+30%"));
            A(new SkillDef("PSA02","전사의 본능", AttributeType.Haste,  AP, SkillType.Passive,0,"3연속 시 4번째 자동 크리"));
            A(new SkillDef("PSA03","수호자의 맹세",AttributeType.Light, AP, SkillType.Passive,0,"합체 시 B 피해 25% 대신 받기"));
            A(new SkillDef("PSA04","광전사의 피", AttributeType.Fire,   AP, SkillType.Passive,0,"HP50%↓ 공+20%, 공속+15%"));
            A(new SkillDef("PSA05","대지의 축복", AttributeType.Earth,  AP, SkillType.Passive,0,"지면 위 방어+10%, HP+5%/초"));
            A(new SkillDef("PSA06","질풍의 발",   AttributeType.Wind,   AP, SkillType.Passive,0,"이속+8%, 대쉬 쿨-20%"));
            A(new SkillDef("PSA07","흡혈귀의 갈증",AttributeType.Dark,  AP, SkillType.Passive,0,"전 공격 흡혈 5%"));
            A(new SkillDef("PSA08","뇌전 반사",   AttributeType.Lightning,AP,SkillType.Passive,0,"피격 시 20% 반격 번개"));
            A(new SkillDef("PSA09","독 내성",     AttributeType.Poison, AP, SkillType.Passive,0,"독/저주 면역, 독 적 추가"));
            A(new SkillDef("PSA10","중력 지배",   AttributeType.Gravity,AP, SkillType.Passive,0,"낙하 면역, 낙하 충격파"));
            A(new SkillDef("PSA11","행운아",      AttributeType.Luck,   AP, SkillType.Passive,0,"전 확률 효과 +5%"));
            A(new SkillDef("PSA12","재생력",      AttributeType.Regen,  AP, SkillType.Passive,0,"5초마다 HP 3%"));

            // ── B 전용 액티브 24 (SKB) ──
            A(new SkillDef("SKB01","파이어볼",     AttributeType.Fire,     BA, SkillType.Projectile,4, "화염구, 폭발+화상", 10));
            A(new SkillDef("SKB02","화염 기둥",     AttributeType.Fire,     BA, SkillType.Area,      10,"지정 지점 3초 유지", 18){ AreaRadius=2.5f, Duration=3 });
            A(new SkillDef("SKB03","아이스 애로우", AttributeType.Ice,      BA, SkillType.Projectile,3, "둔화+빙결 확률", 8));
            A(new SkillDef("SKB04","빙결 감옥",     AttributeType.Ice,      BA, SkillType.CC,        12,"단일 적 빙결(2초)", 20){ Duration=2, AreaRadius=1 });
            A(new SkillDef("SKB05","체인 라이트닝", AttributeType.Lightning,BA, SkillType.Projectile,6, "최대 3체 연쇄", 15){ ChainCount=3 });
            A(new SkillDef("SKB06","썬더 스톰",     AttributeType.Lightning,BA, SkillType.Area,      15,"영역 낙뢰(3회)", 30){ AreaRadius=4 });
            A(new SkillDef("SKB07","워터 힐",       AttributeType.Water,    BA, SkillType.Heal,      10,"A HP 20% 회복", 25){ HealAmount=100 });
            A(new SkillDef("SKB08","조류 소환",     AttributeType.Water,    BA, SkillType.CC,        8, "물결 밀기+둔화", 18){ AreaRadius=3 });
            A(new SkillDef("SKB09","에어 블래스트", AttributeType.Wind,     BA, SkillType.Projectile,4, "공기 탄환 3발", 12));
            A(new SkillDef("SKB10","토네이도",      AttributeType.Wind,     BA, SkillType.Area,      15,"회오리, 끌어당기기", 30){ AreaRadius=3 });
            A(new SkillDef("SKB11","홀리 볼트",     AttributeType.Light,    BA, SkillType.Projectile,4, "언데드 특효", 12));
            A(new SkillDef("SKB12","신성 결계",     AttributeType.Light,    BA, SkillType.Defense,   18,"영역 보호막(5초)", 35){ Duration=5 });
            A(new SkillDef("SKB13","다크 볼트",     AttributeType.Dark,     BA, SkillType.Projectile,5, "흡혈 적용", 14));
            A(new SkillDef("SKB14","공포의 시선",   AttributeType.Dark,     BA, SkillType.CC,        15,"전방 적 공포(2초)", 25){ Duration=2, AreaRadius=4 });
            A(new SkillDef("SKB15","독 구름",       AttributeType.Poison,   BA, SkillType.Area,      10,"독 안개(4초)", 20){ AreaRadius=3, Duration=4 });
            A(new SkillDef("SKB16","맹독 화살",     AttributeType.Poison,   BA, SkillType.Projectile,5, "3중첩 마비", 14));
            A(new SkillDef("SKB17","저주의 손",     AttributeType.Curse,    BA, SkillType.Debuff,    12,"적 방어 30%↓(5초)", 18){ Duration=5, Magnitude=0.3f });
            A(new SkillDef("SKB18","역병",          AttributeType.Curse,    BA, SkillType.Area,      18,"DoT+치유↓", 30){ AreaRadius=3, Duration=5 });
            A(new SkillDef("SKB19","바위 투사체",   AttributeType.Rock,     BA, SkillType.Projectile,6, "넉다운 확률", 16));
            A(new SkillDef("SKB20","시간 둔화",     AttributeType.Time,     BA, SkillType.CC,        15,"범위 50% 슬로우(3초)", 25){ AreaRadius=4, Duration=3, Magnitude=0.5f });
            A(new SkillDef("SKB21","중력장",        AttributeType.Gravity,  BA, SkillType.CC,        15,"끌어당기기(3초)", 25){ AreaRadius=4, Duration=3 });
            A(new SkillDef("SKB22","집중 시전",     AttributeType.Focus,    BA, SkillType.Buff,      12,"다음 스킬 2배(6초내)", 20){ Duration=6, Magnitude=1f });
            A(new SkillDef("SKB23","마나 폭발",     AttributeType.Absorb,   BA, SkillType.Area,      20,"마나30% 소모 비례 피해", 0){ AreaRadius=4 });
            A(new SkillDef("SKB24","연쇄 마법진",   AttributeType.Chain,    BA, SkillType.Setup,     18,"통과 시 연쇄 폭발", 28){ ChainCount=3 });

            // ── B 전용 패시브 12 (PSB) ──
            A(new SkillDef("PSB01","마나 순환",   AttributeType.Water,  BP, SkillType.Passive,0,"스킬 5% 마나 미소모"));
            A(new SkillDef("PSB02","집중력",      AttributeType.Focus,  BP, SkillType.Passive,0,"전 캐스팅 -15%"));
            A(new SkillDef("PSB03","원소 친화",   AttributeType.Fire,   BP, SkillType.Passive,0,"원소 스킬 +10%"));
            A(new SkillDef("PSB04","빙결 마스터", AttributeType.Ice,    BP, SkillType.Passive,0,"빙결 지속 +50%"));
            A(new SkillDef("PSB05","뇌전 전도",   AttributeType.Lightning,BP,SkillType.Passive,0,"감전 연쇄 +2"));
            A(new SkillDef("PSB06","치유의 손길", AttributeType.Light,  BP, SkillType.Passive,0,"전 회복 +20%"));
            A(new SkillDef("PSB07","암흑 공명",   AttributeType.Dark,   BP, SkillType.Passive,0,"흡혈 +10%, 어둠 쿨-15%"));
            A(new SkillDef("PSB08","독술사",      AttributeType.Poison, BP, SkillType.Passive,0,"독 DoT +30%"));
            A(new SkillDef("PSB09","시간 지배자", AttributeType.Time,   BP, SkillType.Passive,0,"전 쿨 -10%"));
            A(new SkillDef("PSB10","마력 흡수",   AttributeType.Absorb, BP, SkillType.Passive,0,"처치 시 마나 8%"));
            A(new SkillDef("PSB11","성장 촉진",   AttributeType.Growth, BP, SkillType.Passive,0,"룬/재료 드롭 +10%"));
            A(new SkillDef("PSB12","수호 본능",   AttributeType.Holy,   BP, SkillType.Passive,0,"HP15%↓ 자동 보호막(60초쿨)"));

            return d;
        }
    }
}
