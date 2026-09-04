// Enums.cs — 게임 전역 열거형 (GDD 01/02 반영)
namespace BroDungeon.Data
{
    /// <summary>플레이 캐릭터. A=거구의 전사, B=다리가 불편한 법사.</summary>
    public enum CharacterType { A_Warrior, B_Mage }

    /// <summary>합체/분리 상태 (문서 01-3). 다운은 분리 중 B 단독 HP 0.</summary>
    public enum MergeState { Merged, Separated }

    /// <summary>B 생존 상태 (I-06: 단독 0 = 즉사 아님, 다운).</summary>
    public enum DownState { Alive, Down, Dead }

    /// <summary>속성 계열 4종 (문서 02-1).</summary>
    public enum AttributeFamily { Element, LightDark, Enhance, Tactic }

    /// <summary>24속성. 룬 문자는 비주얼 모티프(I-03).</summary>
    public enum AttributeType
    {
        // 원소 6
        Fire, Ice, Lightning, Water, Wind, Earth,
        // 빛/어둠 4
        Light, Dark, Holy, Curse,
        // 강화 8
        Haste, Regen, Focus, Endure, Pierce, Absorb, Luck, Growth,
        // 전술 6
        Rock, Poison, Explosion, Time, Gravity, Chain,
        None
    }

    /// <summary>아이템/스킬 등급.</summary>
    public enum Grade { Common, Rare, Legendary, Cursed }

    /// <summary>장착 슬롯 (문서 02-3 / 성장).</summary>
    public enum EquipSlot
    {
        A_Weapon, A_Armor, A_Accessory,   // WPA / ARA / ACA
        B_Weapon, B_Book, B_Accessory,    // WPB / BKB / ACB
        Gadget                            // 공유 가젯
    }

    /// <summary>아이템 분류 접두어 (I-01).</summary>
    public enum ItemCategory { WPA, ARA, ACA, WPB, BKB, ACB, CA, CB }

    /// <summary>스킬 타입 (문서 02-4 표).</summary>
    public enum SkillType { Attack, Dash, Defense, Area, Move, Util, Projectile, CC, Heal, Buff, Debuff, Setup, Passive }

    /// <summary>스킬 소유 (액티브/패시브 × A/B). 접두어 SKA/PSA/SKB/PSB.</summary>
    public enum SkillOwner { A_Active, A_Passive, B_Active, B_Passive }

    /// <summary>상태이상 (속성 효과에서 파생).</summary>
    public enum StatusType
    {
        Burn,       // 화상 DoT
        Slow,       // 둔화
        Freeze,     // 빙결(행동불가)
        Shock,      // 감전
        Poison,     // 독 DoT
        Curse,      // 방어↓ 등 저주
        Stun,       // 기절/넉다운
        Fear,       // 공포
        Bleed,      // 출혈
        Shield,     // 보호막(버프)
        Haste,      // 공속/이속 버프
        Pull,       // 끌어당김
        Silence,    // 스킬 봉인
        Invuln      // 무적
    }

    /// <summary>방 유형 (문서 03-1).</summary>
    public enum RoomType { Combat, Puzzle, Trap, Event, Boss, Start }

    /// <summary>이벤트 방 세부 (문서 03-1).</summary>
    public enum EventRoomType { Altar, Shop, Rest, RuneLab, NPC, SacrificeAltar, TwinAltar }

    /// <summary>제단 선택지 풀 (문서 05-2).</summary>
    public enum AltarPool { A_Stat, B_Blessing, C_Permanent }

    /// <summary>재화 (문서 05-7).</summary>
    public enum Currency
    {
        Gold, Sacrifice, EnhanceStone, HighEnhanceStone,
        ManaCrystal, RuneShard, BossMaterial, DustCrystal, LabyrinthCrystal
    }

    /// <summary>몬스터 행동 타입 (문서 04-1).</summary>
    public enum EnemyType { Melee, Ranged, Flying, Charge, Ambush, Support, Defense, Setup, Special, Elite, Boss }

    /// <summary>게임 모드.</summary>
    public enum GameMode { Story, Endless, Challenge }

    /// <summary>난이도 (문서 06-6).</summary>
    public enum Difficulty { Story, Normal, Hard, Trial }

    /// <summary>네트워크 모드 (문서 01-4).</summary>
    public enum NetMode { Single, CoopHost, CoopClient }
}
