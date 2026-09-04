# BroDungeon 작업 일지

> 기준일: 2026-06-03 | 엔진: Unity 2022.3.52f1 LTS 이상 (최신 미사용) | 언어: C# 9 호환
> 게임명 미정(I-08) → 임시 네임스페이스/폴더 `BroDungeon`

---

## 1차 작업 — 코드 기반 + 데이터 시드 전수 구축 (완료)

기획서 6개 문서(00~06)를 분석하여, 문서 06장 5절 "스크립트 구조(40+)" 아키텍처를
그대로 따르는 **코드 골격 + 전체 데이터 시드**를 구축. 총 58개 `.cs` 생성.

### 폴더 구조 (`Assets/Scripts/`)

```
Data/        Enums, Constants, SaveData              — 열거형/상수/세이브 구조
Database/    AttributeTable, SkillTable, ItemTable,  — md 표 전수 인코딩
             RuneTable, RelicGadgetTable, MonsterTable,
             EndlessTables, DifficultyTable
Core/        GameManager, SaveManager, InputManager,  — 싱글턴 매니저
             AudioManager, GameBootstrap
Utilities/   EventBus, ObjectPool, Timer             — 옵저버/풀/타이머
Characters/  CharacterBase, WarriorController(A),     — 캐릭터/합체메카닉
             MageController(B), MergeSystem,
             ThrowSystem, BondSystem
Combat/      ICombatant, DamageFormula, CombatManager,— 전투
             StatusEffect, ComboSystem, SkillSystem,
             ParrySystem, Projectile, ProjectilePool
Dungeon/     DungeonGenerator, RoomManager,           — 던전/함정/퍼즐/제단/날씨
             TrapTable, TrapSystem, PuzzleTable,
             PuzzleSystem, AltarSystem, WeatherSystem
Items/       InventorySystem, EquipmentSystem,        — 아이템/시너지/제작
             RuneSystem, CraftingSystem, LootSystem,
             CurrencyManager, SynergyCalculator
AI/          StateMachine, EnemyBase, EnemyStates,    — 적/보스 FSM
             BossBase
UI/          HUDManager, UIManagers                   — HUD/메뉴/제단/미니맵 골격
Hub/         HubManager, NPCManager                   — 거점/NPC 골격
Network/     NetworkManager                           — Netcode 인터페이스 골격
```

### 데이터 시드 — md 표 전수 인코딩 (완료)

| 데이터 | 개수 | 파일 | 문서 |
|--------|------|------|------|
| 속성 | 24 (룬·계열·시너지참여 18) | AttributeTable | 02-1 |
| 스킬 | 72 (SKA24/PSA12/SKB24/PSB12) | SkillTable | 02-4 |
| 아이템 | 72 + 저주 12 | ItemTable | 02-3, 02-7 |
| 룬 조합/부여 | 12 / 8 | RuneTable | 02-2 |
| 유물 / 가젯 | 24 / 12 | RelicGadgetTable | 02-5,6 |
| 몬스터 / 보스 | 45 / 6 | MonsterTable | 04 |
| 함정 / 퍼즐 | 35 / 30 | TrapTable / PuzzleTable | 03 |
| 날씨 / 변이 | 16 / 18 | WeatherSystem / EndlessTables | 05 |
| 도전던전 / 도전과제 | 12 / 9 | EndlessTables | 05 |
| 난이도 | 4 | DifficultyTable | 06-6 |

### 핵심 로직 구현 (완료)

- **합체/분리**: MergeSystem FSM, 거리 디버프, 자동 합체 복귀
- **던지기**: ThrowSystem 차지 → 방향 발사
- **유대**: BondSystem 5등급, 스탯/분리완화/콤보 보너스 (문서 01-6)
- **다운/리바이브(I-06)**: B 단독 0 = 8초 다운 → A 회수 시 HP30% 부활 [TBD 수치]
- **데미지 공식**: 문서 01-5/06-4 그대로 (`(기본+장비+제단)×(1+스킬)×(1+속성)×(1+유대)×크리×(1-방어율)`)
- **시너지(I-05/I-07)**: 아이템3+스킬3+룬2 합산, 강화 8종 세트 제외
- **절차적 생성**: 시드 기반(멀티 동기화 대비), 방 유형 가중치, 보스전 제단/휴식 보장
- **세이브**: JSON 3유형(영구/런/보관함), 런 세이브 1회용(스컴 방지)

### 반영된 점검 이슈 (문서 00)

- I-01 데이터 ID 접두어 전면 적용 (WPA/ARA/ACA/SKA/PSA/WPB/BKB/ACB/SKB/PSB/CA/CB)
- I-02 24속성(원소6/빛어둠4/강화8/전술6)
- I-03 룬=비주얼 모티프
- I-05 시너지 합산 소스 (룬 가중치는 `[TBD]`)
- I-06 다운/리바이브
- I-07 세트 시너지 18종 한정

### TBD (플레이테스트 확정 — `Constants.cs`에 잠정값)

- B 다운 8초 / 리바이브 30% / A 단독 디버프 -20%
- 제물 경제 안B (1런 획득 ~1,400)
- 시너지 룬 합산 가중치 (1 vs 0.5)

---

## 미완 / 다음 단계

| 항목 | 상태 |
|------|------|
| 아트/사운드 에셋 (스프라이트·BGM·SFX) | 코드 슬롯만 준비 |
| Netcode 실제 동기화 | 인터페이스 골격만 (W12~14) |
| 프리팹/씬 와이어링 | GameBootstrap 연결 지점 마련 |
| 밸런스 플레이테스트 | TBD 수치 확정 대기 |

---

## 2차 작업 — 2 → 3 → 1 (완료)

### (2) 플레이 슬라이스 — 합체/분리 + 1구역 전투

아트 에셋 없이 런타임에 씬을 빌드하는 방식으로 구현(즉시 플레이 가능).

| 파일 | 역할 |
|------|------|
| `Slice/SliceBootstrap.cs` | 지면·발판·A·B·적·매니저·입력을 코드로 생성 + 디버그 HUD(OnGUI) |
| `Slice/SpriteFactory.cs` | 단색 스프라이트 런타임 생성(플레이스홀더, PPU=16) |
| `Slice/CameraFollow.cs` | A 추적 직교 카메라 |

- 런타임 레이어(Ground6/Player8/Enemy9) 정수 직접 할당 → 프로젝트 설정 불필요
- 조작: ←→ 이동 / Space 점프 / LCtrl 대쉬 / 좌클릭 공격 / 우클릭 방어 / 1·2 A스킬 / Q·E B스킬 / LShift 합체·분리 / Tab 조작전환 / G 던지기
- 부수 수정: MergeSystem 분리 직후 재합체 유예(0.5s)+B 이격, WarriorController 접지 판정 거리 노출

### (3) SO 에디터 툴 — 시드 테이블 → ScriptableObject .asset

| 파일 | 역할 |
|------|------|
| `Data/SO/GameDataSO.cs` | ItemSO/SkillSO/MonsterSO/BossSO/RelicSO/GadgetSO (CreateAssetMenu) |
| `Editor/DataAssetGenerator.cs` | 메뉴 `BroDungeon ▸ Generate Data Assets` — 정적 테이블 → `Assets/Resources/Data/...` .asset 생성·갱신(GUID 보존) |
| `Editor/BroDungeon.Editor.asmdef` | Editor 전용 어셈블리(런타임 BroDungeon 참조) |

### (1) Unity 컴파일 점검 + Boot 씬

- **프로젝트 스캐폴딩**: `ProjectSettings/ProjectVersion.txt`(2022.3.52f1), `Packages/manifest.json`(2D/UGUI/TMP/physics2d 등). → 폴더를 Unity Hub로 바로 열기 가능
- **씬 자동 생성 툴**: `Editor/SceneSetupTool.cs` — 메뉴 `BroDungeon ▸ Create Slice Scene` / `Create Boot Scene`
- **정적 검토로 수정한 컴파일 오류 3건**:
  1. 저주장비(CA/CB) 생성자 인자 개수·타입 오류 → 수정
  2. SkillSystem switch `when` 가드 → if 분기로 정리
  3. PuzzleSystem `BondSystem` using 누락 → 추가
- ⚠️ 이 PC에 Unity 미설치 → 실제 배치모드 컴파일 미실행. 정적 검토 기반. Unity에서 최초 임포트 시 잔여 경고/에러 확인 필요.

### 실행 방법

1. Unity Hub에서 `brodungeon` 폴더 열기 (2022.3.52f1)
2. 메뉴 `BroDungeon ▸ Create Slice Scene` → 생성된 `Assets/Scenes/Slice.unity` 에서 ▶ Play
3. (선택) `BroDungeon ▸ Generate Data Assets` 로 ScriptableObject 에셋 생성

---

## 3차 작업 — 스킬/장비 효과의 실제 스탯 반영 (완료)

장비 효과가 표시 문자열일 뿐이던 문제를 **스탯 집계 시스템**으로 실제 전투에 연결.

### 신규/변경 파일

| 파일 | 역할 |
|------|------|
| `Combat/Stats.cs` (신규) | `StatType` 18종, `StatModifier`, `StatSheet`(최종값 = (base+flat)×(1+pct)) |
| `Items/ItemEffectParser.cs` (신규) | 효과 문자열("공+15%") → StatModifier 변환(재인코딩 없이 md 표 연결) |
| `Characters/CharacterBase.cs` | StatSheet 통합, 파생 스탯 헬퍼(FinalAttackBase/Crit/Pierce), `RecomputeDerived()`(최대 HP 재계산) |
| `Characters/WarriorController.cs` | 공격/이속이 스탯 시트 기반으로, 방어=base.Defense 활용 |
| `Characters/MageController.cs` | 최대 마나 스탯 재계산(OnDerivedRecomputed), 캐스팅/쿨감 스탯 파생 |
| `Combat/SkillSystem.cs` | 스킬 데미지가 캐릭터 스탯(공격/크리/관통/유대) 반영, 쿨감 합산 |
| `Items/EquipmentSystem.cs` | 장착 효과 파싱→StatSheet 집계, 시너지+제단/유물 외부 모디파이어 단일 재계산 경로 |
| `Characters/MergeSystem.cs` | 유대 보너스를 ExternalAtkMultiplier 이중적용→`BondBonus`(데미지 (1+유대) 항)로 정리 |
| `Slice/SliceBootstrap.cs` | 데모: A에 화염의 대검 장착(공+15% & 불 2세트 시너지 발동) |
| `Slice/SelfTest.cs` (신규) | 파서/데미지/시너지/스탯/데이터무결성 런타임 검증(PASS/FAIL 로그) |

### 데미지 흐름 (이제 실제 반영)

```
장비 효과 문자열 → ItemEffectParser → StatSheet 집계
→ FinalAttackBase=(baseAtk+flat)×(1+atk%)  → AttackRequest
→ DamageFormula: (기본+제단)×(1+스킬)×(1+시너지)×(1+유대)×크리×(1-방어율)
```

### 🐛 직접 찾아 고친 버그

1. **저주 디메리트 오역** — `ItemEffectParser`가 "체력 1"/"마나 2배 소비" 같은 디메리트를 +스탯 버프로 잘못 해석.
   → 저주 `Curse` 문자열은 파싱 제외(전용 핸들러 TODO), 이로운 효과(BaseEffect/Special)만 파싱.
2. **유대 보너스 이중 적용** — MergeSystem이 `ExternalAtkMultiplier`에 유대를 곱하는데 데미지 공식엔 (1+유대) 항이 또 있어 중복.
   → 유대는 `BondBonus`(공식 항)로 단일화, 합체 시에만 부여·분리 시 0.
3. **부활 회복 가드** — (2차에서 발견) `Heal`의 생존 가드로 다운 부활 시 HP가 안 채워짐 → `SetCurrentHp` 추가.
4. **분리 직후 즉시 재합체** — (2차) B가 A 등 위치(접촉 반경 내)에서 분리돼 바로 재합체 → 유예 0.5s+이격.

### SelfTest 기대 결과(논리 검증 완료)

- 파서 공+15%→0.15 / 공속·공 비혼동 / 복합 분리 ✓
- FinalAttack(50, +15%)=57.5, FinalMaxHp(500,+20%)=600 ✓
- 데미지(공50,방100)=25, 방어율=0.5 ✓
- 시너지 Fire2=+5%, 강화(Haste)는 시너지 제외 ✓
- 데이터: 아이템 84 / 스킬 72 / 속성 24 / 몬스터 44 + 보스 6 ✓

> ⚠️ Unity 미설치로 실제 Play 실행은 미검증. 위 SelfTest를 Play 시 Console에서 확인 필요.

---

## 4차 작업 — 제단 스탯 연결 + 저주 장비 디메리트 핸들러 (완료)

### (A) 제단 A/C풀 → 실제 스탯

`Dungeon/AltarSystem.cs` 전면 재작성.

- A풀(런 한정 9종)·C풀(영구 8종 + 비스탯 2종)을 **구조화된 StatEntry**로 정의(스탯/범위/대상/%여부)
- 선택 시 `EquipmentSystem.AddExternalModifier`로 실제 StatModifier 반영(대상: A/B/Both)
- C풀은 `PermanentData.permUpgrades`에 단계 누적·저장 + `ApplyPermanentUpgrades()`로 런 시작 시 재적용
- B풀(축복 15종)은 특수 효과라 런 버프 문자열 유지(개별 핸들러 TODO)
- 캐스팅/쿨감은 감소가 이득 → 음수/CDR로 올바르게 매핑

### (B) 저주 장비 디메리트 핸들러

`Items/CursedItemHandler.cs` 신규.

- 이로운 일반 스탯(공+40% 등)은 EquipmentSystem이 BaseEffect/Special 파싱으로 이미 적용
- 핸들러는 **파싱 불가 효과 + 디메리트** 전담:
  - 스탯형: 방어0(CA03), 최대마나-40%(CB02), 체력≈1(CB03), 전마법2배(CB03), 쿨감+50%(CB06) → 공급자(provider)로 EquipmentSystem에 합산(장착/해제 대칭)
  - 런타임: 회복50%↓·회복불가(CA02/CA06 `HealMultiplier`), 마나2배소비(CB01 `ManaCostMultiplier`), 골드/제물3배·드롭-50%(CA05 `LootSystem`), 저체력분노 최대+80%(CA06 `ExtraAttackPct`), 분리스킬봉인(CB04), 주기적 봉인 3분(CB06), 비전투 HP드레인(CA01)
- `EquipmentSystem`에 **동적 공급자(RegisterProvider)** 패턴 추가 → 저주 해제 시 정확히 제거
- 신규 멤버: `CharacterBase.HealMultiplier`/`ExtraAttackPct`(virtual), `MageController.ManaCostMultiplier`/`SkillLockWhenSeparated`/`IsSeparatedState`, `WarriorController.LowHpRageActive`/`MaxStamina`, `LootSystem.gainMultiplier`

### 🐛 직접 찾아 고친 버그

1. **공급자 등록 순서** — CursedItemHandler가 Awake(참조 미설정) 시점에 provider 등록 → 무효. `EnsureRegistered()` 지연 등록으로 수정.
2. **'처치 시' 효과 오역** — 파서가 "처치마나20%"/"처치HP5%"를 최대 마나/HP 증가로 오해석. 부정 룩비하인드 `(?<!처치)` 추가로 차단.
3. **스태미나 최대치 고정** — 제단/스탯의 스태미나 증가가 무시되던 문제. `WarriorController.MaxStamina`(=base+StaminaFlat)로 회복 상한·HUD 연동.
4. (이전 누적) 저주 디메리트 오역·유대 이중적용·부활 회복가드·분리 즉시 재합체.

### SelfTest 추가 검증(논리 확인)

- 저주: 방어0 / 체력≈1 / 마법2배(FinalAttack30→60) / 쿨감 상한 0.8 ✓
- 제단: Generate(seed) 3택 + A풀 스탯 모디파이어 존재 ✓

### 미완 / 다음 단계

- (5차에서 해결) ~~런 라이프사이클 연결~~, ~~CA04 조건부 공격~~
- CB05(시야/맵 표시) — 카메라·미니맵 단계에서 구현
- CA01 비전투 HP드레인의 '전투 상태' 게이팅
- 파서 한계: "처치마나 회복%" 같은 on-kill 효과는 스탯 미반영(전용 효과 핸들러 필요)
- 보스 페이즈 타임라인, 적 종별 고유 행동, 아트/사운드, Netcode 동기화

---

## 5차 작업 — 런 라이프사이클 연결 + CA04 조건부 공격 (완료)

### 런 라이프사이클

`Core/RunManager.cs` 신규 — 런 시작/종료를 단일 오케스트레이션.

- **StartRun(mode, seed)**: `GameManager.ResetForNewRun` → 런버프 제거(`ClearExternal` A/B) → 영구강화 재적용(`AltarSystem.ApplyPermanentUpgrades`) → 유대 로드 → `RoomManager.StartRun`
- **OnGameOver(EventBus 구독)**: 런버프 제거 → 사망 유실(`InventorySystem`/`CurrencyManager.ApplyDeathLoss`) → 유대 영구 저장
- **OnReturnToHub()**: 거점 복귀 시 유대 기록 + 영구 저장
- `GameManager.ResetForNewRun(mode)` 추가, `GameBootstrap`이 모든 참조 와이어링 + RunManager로 런 시작

### CA04 합체/분리 조건부 공격 (근사 → 정확)

- `WarriorController.CursedConditionalAtkPct` 추가 → `ExtraAttackPct`에 합산
- `CursedItemHandler`가 `MergeStateChangedEvent` 구독 → 분리 +50% / 합체 -20% 토글
- 장착 시 현재 합체 상태 기준 초기값 설정

### 🐛 직접 찾아 고친 버그

1. **죽은 런 부활** — `GameManager.ApplyDeathLoss`가 게임오버 시 런을 `SaveRun`하여, 다음 실행에 죽은 런이 이어지던 버그. → 게임오버는 런 종료이므로 `DeleteRun`으로 수정(유실/영구저장은 RunManager가 담당).
2. **CA04 근사치** — 평균 +15% 고정이던 것을 합체/분리 실제 상태 토글로 교체.

### 검증

- RunManager StartRun 순서: ClearExternal → ApplyPermanentUpgrades 순으로 영구강화가 정확히 1회 재적용됨(논리 확인)
- CA04: 분리 시 ExtraAttackPct +0.5, 합체 시 -0.2 (이벤트 구독 확인)

### 다음 단계

- (6차에서 해결) ~~CA01 전투상태 게이팅~~, ~~B풀 축복~~, ~~보스 페이즈 타임라인~~, ~~적 종별 행동~~
- CB05 시야/맵 표시(카메라·미니맵), 아트/사운드, Netcode 동기화

---

## 6차 작업 — 전투상태·축복·적 종별 행동·보스 패턴 (자율 구현 + 버그 점검)

### 전투 상태 추적 (CA01 게이팅)

- `Combat/CombatState.cs` 신규 — `CombatManager.ProcessAttack`에서 `Mark()`, 마지막 전투 후 4초까지 `InCombat`
- CA01(폭식의 대검) 비전투 HP 드레인을 `CombatState.OutOfCombat`로 정확히 게이팅

### B풀 축복 효과 연결

- `AltarSystem`에 축복→스탯 매핑 추가: 전사/마법사(공격), 수호(방어), 신속(이속), 불사(HP), 치명(크리), 탐험가(드롭)
- 기존 제단 파이프라인(`ApplyModifiers`) 재사용 → 자동 적용. 비스탯 축복(가호/총애/감정)은 런 버프 문자열 유지

### 적 종별 행동 (공통 FSM → 종별)

- `EnemyBase` 확장 + `EnemyStates`에 `RangedState` 추가
- **원거리/지원**: 선호 거리 유지(`MaintainDistance`) + 투사체(`PerformRangedAttack`) / 아군 회복(`PerformSupport`)
- **비행**: 중력 0 부유 + 2D 추적 이동
- **돌진**: 윈드업 후 `DoCharge` 가속 돌진
- **방어형**: 기본 방어 2배
- 종별 시작/피격/탐지 상태 전환을 `IsRangedKind` 기준으로 분기

### 보스 페이즈 패턴 시스템

- `AI/BossPattern.cs` 신규 — `BossActionType` 8종(근접호/광역/부채꼴/조준/빔/보호막/돌진/소환) + `BossPatternLibrary`로 5보스+히든 페이즈 구성
- `BossBase` 전면 강화: **액션 스케줄러**(선택→예고→실행→후딜 반복), 페이즈 수 기반 HP 임계 자동 산출, 제네릭 액션 실행기(투사체 부채꼴/조준, 빔 근사, 광역, 보호막=무적, 돌진, 소환)
- 보스 공격 기준치(atkBase)를 구역별 밸런스(문서 04)로 자동 설정

### 🐛 직접 찾아 고친 버그

1. **`in` rvalue 컴파일 에러** — `ProcessAttack(in MakeReq(...))`처럼 메서드 반환값에 `in` 한정자 사용은 불가. 지역변수 경유로 수정(EnemyBase 2곳).
2. **원거리 적 패트롤 복귀 오류** — 탐지/피격 복귀 시 항상 ChaseState(근접)로 가던 것을 `IsRangedKind`면 RangedState로 분기.
3. (점검) 보스 ShieldUp 중 피해 무효: `status.Invulnerable` 가드 확인.

### SelfTest 추가 검증

- 보스1 3페이즈 / 보스5 4페이즈 / 모든 페이즈 액션 보유 ✓

### 남은 단계

- CB05 시야/맵(카메라·미니맵), 소환 시 SummonId 반영(프리팹 비활성화 패턴)
- 아트/사운드 에셋, Netcode 동기화, 밸런스 플레이테스트

---

## 7차 작업 — 집중 버그 점검 패스

전체 코드를 정밀 검토하여 **저장/런타임 치명 버그 다수**를 발견·수정.

### 🐛🐛 치명 버그 (수정)

1. **JsonUtility가 Dictionary 직렬화 불가** — `SaveData`의 `permUpgrades`/`materials`/`runes`/`currencies`/`keyRebinds`가 전부 Dictionary라 **저장 시 조용히 유실**(제단 영구강화·재료·룬 보관·키 리바인딩 전부 안 됨).
   → `ISerializationCallbackReceiver` + `[NonSerialized] Dictionary` + `[SerializeField] List<StrInt/StrStr>` 백킹으로 직렬화 지원.
2. **storage 미저장** — `SaveManager`가 permanent/run만 저장하고 `storage`(재료/룬/보관함)는 저장·로드 안 함. → `storage.json` 저장/로드 추가.
3. **ProjectilePool 초기화 순서** — `Awake`가 prefab 주입 전에 실행돼 **null 프리팹으로 prewarm 32회 Instantiate**(런타임 생성 시 예외). → 첫 Fire 시 지연 초기화(`EnsurePool`).
4. **투사체 트리거 누락** — 투사체가 Rigidbody2D 없이 transform 이동 → 2D 트리거 충돌 누락 가능. → Kinematic Rigidbody2D 추가.

### 점검 후 정상 확인

- 캐릭터/적/보스 Awake 순서, `in` 인자, 시너지/스탯/저주 역연산, 보스 스케줄러, 방어율·회복 가드 등 — 이상 없음

### 알려진 갭(크래시 아님, 추후 연결)

- `RuneSystem`(런타임 dict) ↔ `SaveData.storage.runes` 미동기 → 룬 보관 영속화 별도 연결 필요
- 보스 소환 `SummonId`가 프리팹 Awake 이후 설정돼 프리팹 기본 몬스터로 스폰(비활성 프리팹 패턴 필요)
- CB05 시야/맵은 카메라·미니맵 선행

---

## 8차 작업 — 2차 정밀 버그 점검

### 🐛 발견·수정

1. **분리 패리가 죽은 코드** — `ParrySystem.TryParry`를 호출하는 곳이 없어 핵심 메카닉(분리 패리)이 피해에 무효과였음.
   → `CharacterBase.TakeDamage`에서 윈도우 열림 시 피해 무효화 + 공격자 경직. `CharacterBase.parry` 주입(Slice/GameBootstrap).
2. **플레이어 저주 방어감소 미적용** — `CharacterBase.Defense`가 장비만 반영하고 `StatusController.DefenseModifier`(저주) 무시. → `×(1 - DefenseModifier)` 추가(적과 동일 규칙).
3. **강화석 0개에도 강화 시도** — `CraftingSystem.Enhance`가 재료 0이어도 진행(클램프로 소모만 0). → 강화석 보유 검사 추가.
4. **마나/HP 바 0 나눗셈** — `HUDManager`에서 MaxMana/MaxHp 0일 때 NaN. → `Mathf.Max(1, …)` 가드.

### 점검 후 정상 확인

- 음수 방어 → `DamageFormula`의 `Max(0)` 가드로 피해 증폭 없음
- 상태이상 만료/스택, 쿨다운 틱, 시너지 max 카운트, 보스 페이즈 임계 — 이상 없음

### 여전한 미구현(기능 갭, 버그 아님)

- 날씨(`WeatherSystem.Active`) 효과가 게임플레이에 미적용(데이터만)
- 함정/퍼즐 런타임(`TrapSystem`/`PuzzleSystem`)은 베이스/스텁
- 코옵 분리 시 A/B 입력 분리 라우팅(현재 동일 축) — Netcode 단계

---

## 9차 작업 — 3차 정밀 버그 점검

### 🐛 발견·수정

1. **던지기가 즉시 무효화** — `ThrowSystem.Release`가 B를 A에서 0.6 거리(자동 합체 반경 1.2 안)에 배치 → 다음 프레임 즉시 재합체로 B가 날아가지 못함.
   → 합체 반경 밖(1.7)으로 배치 + `MergeSystem.NotifyThrown()` 유예(0.6s) + 0방향 가드.
2. **B 사망 후 조작 잠김** — 분리 중 B 조작 상태에서 B가 죽으면 `_controllingB`가 true로 고착돼 A도 조작 불가. → B Dead면 A 조작으로 자동 복귀.
3. **InputManager parry null 위험** — `parry.OnSeparateInput()` 무가드 호출. → `parry?.` 가드.

### 점검 후 정상/무해 확인

- `CharacterBase.AltarAttackBonus`는 미사용(잔재) — 제단 효과는 Stats 경유라 이중계산 없음(무해)
- 투사체 hit/lifetime 동시 해제 → `ObjectPool.Release`의 set 검사로 중복 무해
- DoT 재귀 없음, 음수 방어 가드 정상

### 누적 버그 수정 요약(2~9차)

저주 디메리트 오역 · 유대 이중적용 · 부활 회복가드 · 분리 즉시재합체 · `in` rvalue 컴파일에러 · JsonUtility Dictionary 미직렬화 · storage 미저장 · ProjectilePool null prewarm · 투사체 트리거 RB · 패리 죽은코드 · 플레이어 저주 방어감소 · 강화석 미검사 · HUD 0나눗셈 · **던지기 즉시합체** · B사망 조작잠김.

---

## 10차 작업 — 4차 정밀 버그 점검 + 시너지 참여 속성 정합

전체 코드 영역별 병렬 정밀 검토로 잔존 버그 10건 수정 + GDD↔코드 시너지 정합.

### 🐛 발견·수정 (코드)

1. **보스 사망 프레임 페이즈 오발동** — `BossBase.TakeDamage`가 `CheckPhase` 후 `Die` 호출 → 죽는 순간 최종 페이즈 전환(스케줄러 리셋·`OnPhaseChanged` 연출)이 시체에 발동. → 사망 우선 처리(`if (hp<=0) { Die(); return; }`).
2. **보스 페이즈 역행** — `SetPhase`가 `p < Phase`를 허용 → 보스 회복 시 다음 피격에 페이즈가 거꾸로 내려가며 스케줄러 리셋. → `if (p <= Phase) return;` 단조 증가.
3. **저주 장비 슬롯 덮어쓰기 누수** — `EquipmentSystem.Equip`이 저주 점유 슬롯을 `OnUnequipped` 없이 덮어써 디메리트·공급자·루팅 배율이 영구 잔존. → 저주 슬롯 교체 거부(문서 02-7: 런 중 해제 불가).
4. **저주 공급자 모디파이어 무한 증가** — `CursedItemHandler`가 해제 시 음수 값을 누적 상쇄 → 장착/해제 반복 시 리스트 무한 증가. → `AddStat(.., on)` 정확 제거(`FindIndex`).
5. **합체 중 유대 등급 상승 미반영** — `MergeSystem`이 `Merge()` 시점에만 `BondBonus` 스냅샷 → 합체 유지 중 등급업이 데미지 `(1+유대)` 항에 안 들어감. → `BondChangedEvent` 구독해 합체 중 즉시 갱신.
6. **DoT 스택 무한 누적** — `StatusController.Apply`가 재적용마다 `Stacks++` & 틱피해 `×Stacks` → 불/독 무기 반복 피격 시 DoT 무한 증가. → `Constants.STATUS_MAX_STACKS=5` 상한.
7. **A HP 바 0 나눗셈** — `HUDManager`가 A HP 바에만 `Max(1)` 가드 누락(MaxHp 0 시 NaN). → 가드 추가.
8. **InputManager `merge` null NRE** — `Update`가 `merge`를 무가드 참조(미와이어링 씬 매 프레임 크래시). → early-return 가드.
9. **LootSystem `boss.Attrs` null NRE** — `GrantBossReward`가 null 보스/Attrs에 NPE. → 가드.
10. **RuneSystem.IsDiscovered null NRE** — null combo 역참조. → `c != null &&` 가드.

### 시너지 참여 속성 정합 (GDD ↔ 코드, 18종 확정)

- GDD/`SynergyCalculator` 주석/PROGRESS가 "18종"이라 적혀 있으나 `AttributeTable`은 강화 8속성 **전부** 제외(=16)로 구현돼 불일치. 또 GDD 본문 산술 `원소6+빛어둠4+전술6=18`은 16이므로 모순.
- GDD 의도(강화 중 **액티브 스킬 보유 속성**은 시너지 참여) 확정. `SkillTable` 조사 → 강화 중 공격형 액티브 보유 = **관통(SKA20)·흡수(SKB23) 2종**.
- `AttributeTable`에서 관통·흡수 `SynergyMember=true` → 시너지 참여 16+2 = **18종** 정합. GDD 02-1 본문 산술 정정, `SynergyCalculator` 주석·`SelfTest`(관통 참여·속공 제외) 갱신.

### 점검 후 정상/보류

- `MageController.IsAlive` 연산자 우선순위 — Down은 항상 not Dead라 `(A&&B)||C` 결과가 의도와 동일, 버그 아님.
- 투사체 바운스 시 pierce 리셋, 합체마다 유대 +1 파밍 — 엣지/밸런스 사안으로 보류(크래시 아님).
- `SaveData.version` 미영속 — 블록별 직렬화라 버전 미기록. 차기 마이그레이션용, 현재 무해(향후 연결 권장).

> ⚠ Unity 미설치로 배치 컴파일 미실행. 정적 검토 기반.

---

## 11차 작업 — 5차 버그 점검 + 미구현 기능 3종 추가

### 🐛 추가 발견·수정 (미와이어링 NRE + 로직)

1. **RoomManager `GameManager.Instance` 무가드 NRE** — `EnterFloor`는 가드하는데 `OnBossRoomCleared`/`NextFloor`는 무가드 참조. + `FLOORS_PER_ZONE[zone-1]` 범위 초과 위험. → `gm` 캐싱 가드 + `FloorsInZone()` 클램프.
2. **ThrowSystem `merge` null NRE** — `BeginCharge`가 `merge.State` 무가드. → null 가드.
3. **SkillSystem `owner` null NRE** — `Cast`가 `owner.Status` 무가드(입력에서 직접 호출). → `if (owner == null) return false;`.
4. **GameBootstrap `network` null NRE** — `LoadOrStartRun`이 `network.StartSingle()` 무가드(실패 시 이후 와이어링 전부 중단). → null 가드.
5. **AltarSystem 중복 3택** — 1번·3번 선택지가 독립적으로 APool에서 뽑혀 같은 항목 중복 가능. → 1번과 다른 인덱스로 강제.

### ✨ 미구현 기능 추가

**(1) 룬 영속화** (`RuneSystem` + `RunManager`)
- 7차에서 갭으로 남았던 `RuneSystem` ↔ `SaveData.storage.runes` 동기 연결.
- `SaveTo/LoadFrom`(보유 룬 "속성:티어"→개수), `SaveDiscovered/LoadDiscovered`(조합 발견 → `permanent`).
- `RunManager.StartRun`에서 로드, 게임오버·거점복귀 시 `PersistPermanent()`로 저장(유대와 함께).

**(2) 날씨 효과 게임플레이 연동** (`WeatherSystem` 전면 확장)
- 8차에서 "데이터만"이던 환경 변이를 실제 효과로 연결.
- `EquipmentSystem` 공급자 패턴으로 스탯 모디파이어 적용(폭우/폭설/시간왜곡 이속↓, 마력폭풍 마나·마법↑/물리↓, 열파 스태미나↑, 바람축제 이속↑).
- 주기적 HP(독안개 초당 3% 독피해, 꽃가루 초당 3% 회복), 골드/드롭 배율(황금빛·룬공명) — loot 델타 대칭 적용/원복.
- 카메라·지형·적 전역버프가 필요한 변이(안개/지진/정전/중력/번개폭풍/저주의밤)는 데이터 유지 + 로그(차기 단계).

**(3) 함정 런타임 구현** (`TrapSystem`)
- 스텁(`Fire` 빈 동작) + 고아 인터페이스 `ICombatantLike` 제거.
- `Physics2D.OverlapCircle` + `ICombatant.TakeDamage`로 실제 주기적 피해. 합체(무게) 시 `mergedDamageMul` 분기, `oneShot`/`Rearm`, 속성 함정 지원, 중복 타격 방지.

### 검증
- 10차 수정 10건 전부 정상 적용·회귀 없음(독립 재검증 통과).
- 신규 코드 타입/네임스페이스/usings 정적 검토 완료. ⚠ Unity 미설치로 배치 컴파일 미실행.
- (재점검) 11차 신규 코드 적대적 검증: 날씨 loot 델타 라이프사이클·CA05 비충돌, AltarSystem 모듈러 인덱스, 룬 라운드트립 모두 정상. **ThrowSystem.Release의 `A` 무가드 참조 NRE 1건 추가 수정**(merge만 가드했던 잔재).

### 남은 단계
- CB05 시야/맵(카메라·미니맵), 날씨 시각/지형/적 효과, 보스 소환 SummonId 프리팹 패턴, 아트/사운드, Netcode 동기화.
