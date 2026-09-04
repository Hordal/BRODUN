// ItemTable.cs — 72개 아이템 + 12 저주장비 전수 인코딩 (문서 02-3, 02-7)
// WPA/ARA/ACA (A) + WPB/BKB/ACB (B) 각 12 + CA/CB 각 6
using System.Collections.Generic;
using BroDungeon.Data;

namespace BroDungeon.Database
{
    public class ItemDef
    {
        public string Id, Name, BaseEffect, Special, Curse;
        public ItemCategory Category;
        public EquipSlot Slot;
        public AttributeType Attr1, Attr2;
        public Grade Grade;

        public ItemDef(string id, string name, ItemCategory cat, EquipSlot slot, Grade grade,
                       AttributeType a1, string baseEffect, string special = "",
                       AttributeType a2 = AttributeType.None, string curse = "")
        {
            Id = id; Name = name; Category = cat; Slot = slot; Grade = grade;
            Attr1 = a1; Attr2 = a2; BaseEffect = baseEffect; Special = special; Curse = curse;
        }
        public bool IsCursed => Grade == Grade.Cursed;
    }

    public static class ItemTable
    {
        public static readonly Dictionary<string, ItemDef> All = Build();
        public static ItemDef Get(string id) => All.TryGetValue(id, out var v) ? v : null;
        public static IEnumerable<ItemDef> ByCategory(ItemCategory c)
        { foreach (var v in All.Values) if (v.Category == c) yield return v; }

        static Dictionary<string, ItemDef> Build()
        {
            var d = new Dictionary<string, ItemDef>();
            void I(ItemDef x) => d[x.Id] = x;
            var C = Grade.Common; var R = Grade.Rare; var L = Grade.Legendary; var X = Grade.Cursed;
            var WPA = ItemCategory.WPA; var ARA = ItemCategory.ARA; var ACA = ItemCategory.ACA;
            var WPB = ItemCategory.WPB; var BKB = ItemCategory.BKB; var ACB = ItemCategory.ACB;
            var sAW = EquipSlot.A_Weapon; var sAR = EquipSlot.A_Armor; var sAC = EquipSlot.A_Accessory;
            var sBW = EquipSlot.B_Weapon; var sBK = EquipSlot.B_Book; var sBC = EquipSlot.B_Accessory;
            var AT = AttributeType.None; _ = AT;

            // ── A 전용 무기 12 (WPA) ──
            I(new ItemDef("WPA01","화염의 대검",   WPA,sAW,C,AttributeType.Fire,     "공+15%, 화상","3콤보 화염 폭발"));
            I(new ItemDef("WPA02","빙하의 전투도끼",WPA,sAW,C,AttributeType.Ice,      "공+10%, 둔화","빙결 적 +50%"));
            I(new ItemDef("WPA03","뇌신의 워해머", WPA,sAW,C,AttributeType.Lightning,"공+12%, 감전","감전 연쇄"));
            I(new ItemDef("WPA04","대지의 메이스", WPA,sAW,C,AttributeType.Earth,    "공+8%, 방+10%","강공 충격파",AttributeType.Rock));
            I(new ItemDef("WPA05","질풍의 쌍검",   WPA,sAW,R,AttributeType.Wind,     "공속+20%, 이속+10%","대쉬 강화",AttributeType.Haste));
            I(new ItemDef("WPA06","신성한 장검",   WPA,sAW,R,AttributeType.Light,    "공+10%, 언데드 특효","처치 시 회복",AttributeType.Holy));
            I(new ItemDef("WPA07","그림자 단검",   WPA,sAW,R,AttributeType.Dark,     "공+12%, 크리+8%","배후 2배"));
            I(new ItemDef("WPA08","독아의 낫",     WPA,sAW,C,AttributeType.Poison,   "공+10%, 독 DoT","독 3중첩 폭발",AttributeType.Curse));
            I(new ItemDef("WPA09","관통의 창",     WPA,sAW,R,AttributeType.Pierce,   "방무시 25%","고방어 적 추가"));
            I(new ItemDef("WPA10","시간의 검",     WPA,sAW,L,AttributeType.Time,     "공속+15%, 쿨감 10%","5연속 시간감속",AttributeType.Haste));
            I(new ItemDef("WPA11","중력의 대망치", WPA,sAW,L,AttributeType.Gravity,  "공+18%, 넉다운","강공 끌어당기기",AttributeType.Explosion));
            I(new ItemDef("WPA12","연쇄의 쇄도",   WPA,sAW,L,AttributeType.Chain,    "공+10%, 다중 타격","3체+ 바운스",AttributeType.Lightning));

            // ── A 전용 방어구 12 (ARA) ──
            I(new ItemDef("ARA01","화염 판금갑",   ARA,sAR,C,AttributeType.Fire,  "방+12%, 화상 면역","반격 화상"));
            I(new ItemDef("ARA02","빙결 쇄갑",     ARA,sAR,C,AttributeType.Ice,   "방+10%, 빙결 면역","피격 시 둔화"));
            I(new ItemDef("ARA03","뇌전 경갑",     ARA,sAR,C,AttributeType.Lightning,"방+8%, 감전 면역","반격 전기"));
            I(new ItemDef("ARA04","대지의 중갑",   ARA,sAR,R,AttributeType.Earth, "방+18%, 넉백 면역","HP25%↓ 슈퍼아머"));
            I(new ItemDef("ARA05","질풍의 가죽갑", ARA,sAR,R,AttributeType.Wind,  "방+5%, 회피+15%","대쉬 무적 연장"));
            I(new ItemDef("ARA06","신성 법의",     ARA,sAR,R,AttributeType.Light, "방+10%, HP 회복","주변 아군 소량 회복"));
            I(new ItemDef("ARA07","그림자 외투",   ARA,sAR,R,AttributeType.Dark,  "방+8%, 은신↑","분리 시 B 은신 연장"));
            I(new ItemDef("ARA08","독무의 갑옷",   ARA,sAR,C,AttributeType.Poison,"방+10%, 독 면역","피격 시 독 안개"));
            I(new ItemDef("ARA09","인내의 흉갑",   ARA,sAR,C,AttributeType.Endure,"방+12%, 스태미나+20%","소진 시 방어 2배(3초)"));
            I(new ItemDef("ARA10","재생의 갑옷",   ARA,sAR,C,AttributeType.Regen, "방+8%, HP 회복","비전투 시 회복 3배"));
            I(new ItemDef("ARA11","바위 거인 갑옷", ARA,sAR,L,AttributeType.Rock,  "방+20%, CC 저항","넉다운 면역"));
            I(new ItemDef("ARA12","행운의 여행자", ARA,sAR,R,AttributeType.Luck,  "방+5%, 드롭+10%","피격 시 확률 골드"));

            // ── A 전용 장신구 12 (ACA) ──
            I(new ItemDef("ACA01","전사의 분노 반지",ACA,sAC,C,AttributeType.Fire,    "HP30%↓ 시 공+25%"));
            I(new ItemDef("ACA02","빙심의 반지",     ACA,sAC,C,AttributeType.Ice,     "CC 지속+20%"));
            I(new ItemDef("ACA03","번개의 목걸이",   ACA,sAC,C,AttributeType.Lightning,"치확+12%"));
            I(new ItemDef("ACA04","대지의 팔찌",     ACA,sAC,C,AttributeType.Earth,   "최대 HP+15%"));
            I(new ItemDef("ACA05","돌풍의 귀걸이",   ACA,sAC,R,AttributeType.Wind,    "이속+12%, 대쉬거리+20%"));
            I(new ItemDef("ACA06","수호자의 문장",   ACA,sAC,R,AttributeType.Light,   "합체 시 B 피해 -20%"));
            I(new ItemDef("ACA07","암살자의 반지",   ACA,sAC,R,AttributeType.Dark,    "배후 크리티컬 확정"));
            I(new ItemDef("ACA08","관통의 브로치",   ACA,sAC,R,AttributeType.Pierce,  "적 방어 15% 무시(중첩)"));
            I(new ItemDef("ACA09","흡수의 목걸이",   ACA,sAC,C,AttributeType.Absorb,  "처치 시 마나 10% 회복"));
            I(new ItemDef("ACA10","재생의 반지",     ACA,sAC,C,AttributeType.Regen,   "5초마다 HP 2% 회복"));
            I(new ItemDef("ACA11","성장의 부적",     ACA,sAC,C,AttributeType.Growth,  "제물/경험치+15%"));
            I(new ItemDef("ACA12","중력의 완장",     ACA,sAC,R,AttributeType.Gravity, "강공 끌어당기기 범위↑"));

            // ── B 전용 지팡이 12 (WPB) ──
            I(new ItemDef("WPB01","화염의 지팡이",  WPB,sBW,C,AttributeType.Fire,     "마공+15%, 화상","차징 화염 기둥"));
            I(new ItemDef("WPB02","빙하의 지팡이",  WPB,sBW,C,AttributeType.Ice,      "마공+10%, 빙결","빙결 처치 얼음 폭발"));
            I(new ItemDef("WPB03","뇌전의 지팡이",  WPB,sBW,C,AttributeType.Lightning,"마공+12%, 연쇄","감전 추가 피해"));
            I(new ItemDef("WPB04","생명의 지팡이",  WPB,sBW,R,AttributeType.Water,    "치유+25%, 마나 회복","10% 마나 미소모"));
            I(new ItemDef("WPB05","폭풍의 지팡이",  WPB,sBW,C,AttributeType.Wind,     "마공+10%, 넉백↑","공중 적 +50%"));
            I(new ItemDef("WPB06","빛의 지팡이",    WPB,sBW,R,AttributeType.Light,    "마공+10%, 치유↑","보호막 시전"));
            I(new ItemDef("WPB07","암흑의 지팡이",  WPB,sBW,R,AttributeType.Dark,     "마공+15%, 흡혈 10%","처치 어둠 폭발"));
            I(new ItemDef("WPB08","저주의 지팡이",  WPB,sBW,R,AttributeType.Curse,    "마공+12%, 디버프↑","디버프 +30%"));
            I(new ItemDef("WPB09","시간의 지팡이",  WPB,sBW,L,AttributeType.Time,     "쿨-15%, 둔화↑","3연속 시간 정지(1초)"));
            I(new ItemDef("WPB10","독아의 지팡이",  WPB,sBW,C,AttributeType.Poison,   "마공+10%, 독","독 확산"));
            I(new ItemDef("WPB11","연쇄의 지팡이",  WPB,sBW,L,AttributeType.Chain,    "마공+10%, 바운스","투사체 2회 바운스"));
            I(new ItemDef("WPB12","폭발의 지팡이",  WPB,sBW,L,AttributeType.Explosion,"마공+18%, 범위+30%","낮은 사거리 고범위"));

            // ── B 전용 서적 12 (BKB) ──
            I(new ItemDef("BKB01","불의 비전서",     BKB,sBK,C,AttributeType.Fire,     "불 스킬 +20%"));
            I(new ItemDef("BKB02","얼음의 비전서",   BKB,sBK,C,AttributeType.Ice,      "빙결 확률 +15%"));
            I(new ItemDef("BKB03","번개의 비전서",   BKB,sBK,C,AttributeType.Lightning,"연쇄 대상 +1"));
            I(new ItemDef("BKB04","흐르는 물의 서적", BKB,sBK,C,AttributeType.Water,    "마나 +20%, 마나 회복 +15%"));
            I(new ItemDef("BKB05","바람의 서적",     BKB,sBK,C,AttributeType.Wind,     "투사체 속도 +30%, 사거리 +20%"));
            I(new ItemDef("BKB06","빛의 경전",       BKB,sBK,R,AttributeType.Light,    "회복 +25%"));
            I(new ItemDef("BKB07","어둠의 금서",     BKB,sBK,R,AttributeType.Dark,     "흡혈 +15%, 어둠 스킬↑"));
            I(new ItemDef("BKB08","집중의 서적",     BKB,sBK,C,AttributeType.Focus,    "캐스팅 -20%"));
            I(new ItemDef("BKB09","흡수의 마도서",   BKB,sBK,C,AttributeType.Absorb,   "처치 시 마나 15%"));
            I(new ItemDef("BKB10","시간의 연대기",   BKB,sBK,L,AttributeType.Time,     "전 쿨 -10%"));
            I(new ItemDef("BKB11","성장의 학술서",   BKB,sBK,C,AttributeType.Growth,   "제물/경험치 +20%"));
            I(new ItemDef("BKB12","행운의 그림책",   BKB,sBK,R,AttributeType.Luck,     "룬 드롭 +15%"));

            // ── B 전용 장신구 12 (ACB) ──
            I(new ItemDef("ACB01","마력의 오브",     ACB,sBC,C,AttributeType.Water,    "마나 +20%"));
            I(new ItemDef("ACB02","빙정의 브로치",   ACB,sBC,C,AttributeType.Ice,      "빙결 지속 +30%"));
            I(new ItemDef("ACB03","전격의 이어링",   ACB,sBC,C,AttributeType.Lightning,"시전 시 확률 번개"));
            I(new ItemDef("ACB04","화염의 펜던트",   ACB,sBC,C,AttributeType.Fire,     "화상 +25%"));
            I(new ItemDef("ACB05","바람의 날개",     ACB,sBC,R,AttributeType.Wind,     "분리 시 B 이동↑"));
            I(new ItemDef("ACB06","집중의 안경",     ACB,sBC,C,AttributeType.Focus,    "차징 -25%"));
            I(new ItemDef("ACB07","생명의 브로치",   ACB,sBC,R,AttributeType.Regen,    "분리 시 B HP 회복"));
            I(new ItemDef("ACB08","저주의 팔찌",     ACB,sBC,R,AttributeType.Curse,    "디버프 적에 추가 +15%"));
            I(new ItemDef("ACB09","시간의 회중시계", ACB,sBC,L,AttributeType.Time,     "사망 시 1회 부활(던전당)"));
            I(new ItemDef("ACB10","관통의 모노클",   ACB,sBC,R,AttributeType.Pierce,   "마법 방어 무시 20%"));
            I(new ItemDef("ACB11","연쇄의 팔찌",     ACB,sBC,R,AttributeType.Chain,    "투사체 관통 +1"));
            I(new ItemDef("ACB12","성스러운 로자리오",ACB,sBC,R,AttributeType.Holy,    "디버프 지속 -30%"));

            // ── 저주 장비 12 (CA/CB) — 문서 02-7 ──
            I(new ItemDef("CA01","폭식의 대검",   ItemCategory.CA,sAW,X,AttributeType.None,"공+40%, 처치HP5%","",AttributeType.None,"비전투 HP1%/초↓"));
            I(new ItemDef("CA02","피의 갑옷",     ItemCategory.CA,sAR,X,AttributeType.None,"방+35%, 반격","",AttributeType.None,"회복 50%↓"));
            I(new ItemDef("CA03","분노의 투구",   ItemCategory.CA,sAC,X,AttributeType.Haste,"공속+30%, 크리+15%","",AttributeType.None,"방어 0"));
            I(new ItemDef("CA04","고독의 팔찌",   ItemCategory.CA,sAC,X,AttributeType.None,"분리시 공+50%","",AttributeType.None,"합체시 공-20%"));
            I(new ItemDef("CA05","탐욕의 반지",   ItemCategory.CA,sAC,X,AttributeType.Luck,"골드/제물 3배","",AttributeType.None,"드롭 -50%"));
            I(new ItemDef("CA06","광전사의 부적", ItemCategory.CA,sAC,X,AttributeType.Fire,"HP↓ 공↑(최대+80%)","",AttributeType.None,"회복 불가"));
            I(new ItemDef("CB01","폭주의 지팡이", ItemCategory.CB,sBW,X,AttributeType.Explosion,"마공+50%, 범위+30%","",AttributeType.None,"마나 2배 소비"));
            I(new ItemDef("CB02","흡마의 서적",   ItemCategory.CB,sBK,X,AttributeType.Absorb,"처치마나20%, 쿨-25%","",AttributeType.None,"최대마나 -40%"));
            I(new ItemDef("CB03","유리의 오브",   ItemCategory.CB,sBC,X,AttributeType.None,"전 마법 2배","",AttributeType.None,"체력 1"));
            I(new ItemDef("CB04","속박의 목걸이", ItemCategory.CB,sBC,X,AttributeType.None,"합체 자동시전 3배","",AttributeType.None,"분리 스킬 불가"));
            I(new ItemDef("CB05","예언자의 안경", ItemCategory.CB,sBC,X,AttributeType.None,"맵/함정/적 전표시","",AttributeType.None,"시야 -70%"));
            I(new ItemDef("CB06","시간의 저주서", ItemCategory.CB,sBK,X,AttributeType.Time,"전 쿨 -50%","",AttributeType.None,"3분마다 스킬 봉인"));

            return d;
        }
    }
}
