# BroDungeon — 코드 기반 (GDD 구현)

> 기획서(00~06 md)를 기반으로 한 Unity 2022.3.52f1 LTS 코드 골격.
> 게임명 미정(I-08) → 임시 네임스페이스/폴더명 `BroDungeon` 사용.

## 이 코드가 다루는 범위

문서 `06...md` 5절 "스크립트 구조(40+)"와 아키텍처 패턴(싱글턴/FSM/옵저버/오브젝트풀/ScriptableObject DB/커맨드)을
그대로 구현한 **코드 기반 + 전체 데이터 시드**입니다.

| 레이어 | 폴더 | 상태 |
|--------|------|------|
| Data (상수/열거형/세이브) | `Assets/Scripts/Data` | 구현 |
| ScriptableObject 정의 | `Assets/Scripts/Items/SO`, `AI` | 구현 |
| 데이터 시드 (24속성/72아이템/72스킬/24룬+12조합/24유물/12가젯/12저주/45몹/6보스) | `Assets/Scripts/Database` | 구현 (md 표 전수 인코딩) |
| Core (GameManager/Save/Input/Audio/EventBus/Pool/Timer) | `Assets/Scripts/Core`, `Utilities` | 구현 |
| Characters (합체/분리/던지기/유대) | `Assets/Scripts/Characters` | 구현 |
| Combat (데미지공식/콤보/스킬/패리/상태이상/투사체) | `Assets/Scripts/Combat` | 구현 |
| Dungeon (절차생성/방/함정/퍼즐/제단/날씨) | `Assets/Scripts/Dungeon` | 구현 |
| Items (인벤/장비/룬/제작/루팅) | `Assets/Scripts/Items` | 구현 |
| AI (EnemyBase/BossBase/FSM) | `Assets/Scripts/AI` | 구현 |
| UI (HUD/메뉴/제단/미니맵...) | `Assets/Scripts/UI` | 골격 |
| Hub (거점/공방/대장간/NPC) | `Assets/Scripts/Hub` | 골격 |
| Network (Netcode 호스트-클라) | `Assets/Scripts/Network` | 인터페이스 골격 |

## 데이터 ID 접두어 (I-01 반영)
A무기 `WPA` / A방어구 `ARA` / A장신구 `ACA` / A액티브 `SKA` / A패시브 `PSA`
B지팡이 `WPB` / B서적 `BKB` / B장신구 `ACB` / B액티브 `SKB` / B패시브 `PSB`
저주장비 `CA`/`CB`, 유물 `R`, 가젯 `G`, 몬스터 `M`, 함정 `T`, 퍼즐 `P`, 날씨 `W`, 도전던전 `CD`.

## 핵심 공식 (문서 01/06)
```
데미지 = (기본공 + 장비 + 제단) × (1+스킬배율) × (1+속성) × (1+유대) × 크리 × (1-방어율)
방어율 = 방어력 / (방어력 + 100)
```

## 다음 단계 (16주 일정 기준)
- 아트/사운드 에셋 임포트 (W10~11)
- Netcode 동기화 실제 구현 (W12~14)
- 프리팹/씬 와이어링, 밸런스 플레이테스트 (TBD 수치 확정: 다운 8초/리바이브 30%/A디버프 -20%, 제물경제 안B)
