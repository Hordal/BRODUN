// SliceBootstrap.cs — 합체/분리 + 1구역 전투 플레이 슬라이스 (아트 에셋 불필요).
// 사용법: 빈 씬에 빈 GameObject 1개 + 이 컴포넌트 → Play.
//   조작: 좌우 이동 / Space 점프 / LCtrl 대쉬 / 좌클릭 공격 / 우클릭 방어
//         1,2 = A 스킬 / Q,E = B 스킬 / LShift = 합체·분리 전환 / Tab = (분리 중)조작 전환
//         G = (분리 중) B 던지기 차지·발사
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Core;
using BroDungeon.Characters;
using BroDungeon.Combat;
using BroDungeon.AI;
using BroDungeon.Items;

namespace BroDungeon.Slice
{
    public class SliceBootstrap : MonoBehaviour
    {
        // 런타임 레이어 인덱스(프로젝트 설정 불필요, 정수 직접 할당)
        const int L_GROUND = 6, L_PLAYER = 8, L_ENEMY = 9;

        [Header("슬라이스 설정")]
        public int enemyCount = 3;
        public Color groundColor = new Color(0.45f, 0.34f, 0.24f); // 1구역 갈색 석벽
        public Color warriorColor = new Color(0.80f, 0.30f, 0.25f);
        public Color mageColor = new Color(0.30f, 0.45f, 0.85f);
        public Color enemyColor = new Color(0.55f, 0.55f, 0.55f);

        WarriorController _a;
        MageController _b;
        MergeSystem _merge;

        void Start()
        {
            int groundMask = 1 << L_GROUND;
            int enemyMask = 1 << L_ENEMY;

            BuildManagers();
            BuildCamera();
            BuildGround();

            _a = BuildWarrior(groundMask, enemyMask);
            _b = BuildMage(enemyMask);
            var systems = BuildCharacterSystems(_a, _b, enemyMask);
            _merge = systems.merge;

            BuildInput(_a, _b, systems);
            BuildEquipment(_a, _b, systems.aSkills, systems.bSkills);
            for (int i = 0; i < enemyCount; i++)
                BuildEnemy(new Vector3(5 + i * 3f, 0.5f, 0), _a.transform);

            Debug.Log("[Slice] 1구역 전투 슬라이스 준비 완료. WASD/←→ 이동, 좌클릭 공격, LShift 합체/분리.");
        }

        // ── 매니저 싱글턴 ──
        void BuildManagers()
        {
            var go = new GameObject("~Managers");
            go.AddComponent<GameManager>();
            go.AddComponent<CombatManager>();
            go.AddComponent<BondSystem>();
            go.AddComponent<CurrencyManager>();
            go.AddComponent<InventorySystem>();
            go.AddComponent<LootSystem>();
            go.AddComponent<RuneSystem>();
        }

        void BuildCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.06f);
            if (cam.GetComponent<CameraFollow>() == null) cam.gameObject.AddComponent<CameraFollow>();
        }

        void BuildGround()
        {
            var go = new GameObject("Ground") { layer = L_GROUND };
            go.transform.position = new Vector3(0, -2f, 0);
            go.transform.localScale = new Vector3(60, 1, 1);
            SpriteFactory.AttachSprite(go, groundColor, 64, 16, order: -1);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(64f / Constants.PIXELS_PER_UNIT, 1f);

            // 발판 하나 더 (분리/던지기 동선)
            var plat = new GameObject("Platform") { layer = L_GROUND };
            plat.transform.position = new Vector3(8, 1.5f, 0);
            SpriteFactory.AttachSprite(plat, groundColor, 48, 8, order: -1);
            var pcol = plat.AddComponent<BoxCollider2D>();
            pcol.size = new Vector2(48f / Constants.PIXELS_PER_UNIT, 0.5f);
        }

        WarriorController BuildWarrior(int groundMask, int enemyMask)
        {
            var go = new GameObject("A_Warrior") { layer = L_PLAYER };
            go.transform.position = new Vector3(0, 0, 0);
            SpriteFactory.AttachSprite(go, warriorColor, 16, 24, order: 5);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true; rb.gravityScale = 3f;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1.5f);

            var a = go.AddComponent<WarriorController>();
            a.groundMask = groundMask; a.enemyMask = enemyMask;
            a.GameManagerRef = GameManager.Instance;

            // 공격 원점(전방)
            var origin = new GameObject("AttackOrigin").transform;
            origin.SetParent(go.transform);
            origin.localPosition = new Vector3(0.8f, 0, 0);
            a.attackOrigin = origin;
            return a;
        }

        MageController BuildMage(int enemyMask)
        {
            var go = new GameObject("B_Mage") { layer = L_PLAYER };
            go.transform.position = new Vector3(1.5f, 0, 0);
            SpriteFactory.AttachSprite(go, mageColor, 12, 16, order: 6);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true; rb.gravityScale = 3f;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 1f);

            var b = go.AddComponent<MageController>();
            b.enemyMask = enemyMask;
            b.GameManagerRef = GameManager.Instance;
            return b;
        }

        (MergeSystem merge, ThrowSystem throwSys, ParrySystem parry, SkillSystem aSkills, SkillSystem bSkills)
            BuildCharacterSystems(WarriorController a, MageController b, int enemyMask)
        {
            var go = new GameObject("~CharacterSystems");

            var merge = go.AddComponent<MergeSystem>();
            merge.A = a; merge.B = b;
            var mount = new GameObject("BackMount").transform;
            mount.SetParent(a.transform);
            mount.localPosition = new Vector3(-0.2f, 0.9f, 0);
            merge.backMount = mount;

            var throwSys = go.AddComponent<ThrowSystem>();
            throwSys.A = a; throwSys.B = b; throwSys.merge = merge;

            var parry = go.AddComponent<ParrySystem>();
            parry.merge = merge;
            a.parry = parry; b.parry = parry; // 분리 패리 연결

            // 투사체 풀 (B 마법용)
            var pool = BuildProjectilePool();

            var aSkills = a.gameObject.AddComponent<SkillSystem>();
            aSkills.owner = a; aSkills.enemyMask = enemyMask;
            aSkills.Equip("SKA01"); aSkills.Equip("SKA09"); // 화염 베기 / 질풍 대쉬

            var bSkills = b.gameObject.AddComponent<SkillSystem>();
            bSkills.owner = b; bSkills.enemyMask = enemyMask; bSkills.projectilePool = pool;
            bSkills.Equip("SKB01"); bSkills.Equip("SKB03"); // 파이어볼 / 아이스 애로우

            return (merge, throwSys, parry, aSkills, bSkills);
        }

        ProjectilePool BuildProjectilePool()
        {
            // 투사체 템플릿(비활성)
            var tpl = new GameObject("ProjectileTemplate");
            SpriteFactory.AttachSprite(tpl, new Color(1f, 0.6f, 0.2f), 6, 6, order: 7);
            var col = tpl.AddComponent<CircleCollider2D>();
            col.isTrigger = true; col.radius = 0.2f;
            // 이동하는 트리거 콜라이더는 Rigidbody2D(Kinematic)가 있어야 충돌이 안정적으로 감지됨
            var prb = tpl.AddComponent<Rigidbody2D>();
            prb.bodyType = RigidbodyType2D.Kinematic; prb.gravityScale = 0f;
            var proj = tpl.AddComponent<Projectile>();
            proj.speed = 12f; proj.lifetime = 3f;
            tpl.SetActive(false);

            var poolGo = new GameObject("~ProjectilePool");
            var pool = poolGo.AddComponent<ProjectilePool>();
            pool.prefab = proj; pool.prewarm = 16;
            return pool;
        }

        void BuildInput(WarriorController a, MageController b,
            (MergeSystem merge, ThrowSystem throwSys, ParrySystem parry, SkillSystem aSkills, SkillSystem bSkills) s)
        {
            var go = new GameObject("~Input");
            var input = go.AddComponent<InputManager>();
            input.warrior = a; input.mage = b; input.merge = s.merge;
            input.throwSystem = s.throwSys; input.parry = s.parry;
            input.warriorSkills = s.aSkills; input.mageSkills = s.bSkills;
            input.cam = Camera.main;

            // 카메라가 A 추적
            var follow = Camera.main.GetComponent<CameraFollow>();
            if (follow) follow.target = a.transform;
        }

        // 장비 시스템 연동 데모: A에게 시작 무기 장착 → 공+15%, 화염 속성 부여
        void BuildEquipment(WarriorController a, MageController b, Combat.SkillSystem aSkills, Combat.SkillSystem bSkills)
        {
            var go = new GameObject("~Equipment");
            var equip = go.AddComponent<EquipmentSystem>();
            equip.warrior = a; equip.mage = b; equip.warriorSkills = aSkills; equip.mageSkills = bSkills;

            // 저주 장비 핸들러
            var cursed = go.AddComponent<CursedItemHandler>();
            cursed.a = a; cursed.b = b; cursed.merge = _merge; cursed.equipment = equip; cursed.loot = LootSystem.Instance;
            equip.cursedHandler = cursed;
            cursed.EnsureRegistered();

            // 제단(제물로 스탯 강화 — UI 없이 코드로도 호출 가능)
            equip.gameObject.AddComponent<BroDungeon.Dungeon.AltarSystem>().equipment = equip;

            InventorySystem.Instance.Add("WPA01", identified: true); // 화염의 대검
            var entry = InventorySystem.Instance.Bag[InventorySystem.Instance.Bag.Count - 1];
            equip.Equip(entry);
        }

        void BuildEnemy(Vector3 pos, Transform target)
        {
            var go = new GameObject("Enemy_M01") { layer = L_ENEMY };
            go.transform.position = pos;
            SpriteFactory.AttachSprite(go, enemyColor, 16, 16, order: 4);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true; rb.gravityScale = 3f;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            var e = go.AddComponent<EnemyBase>();
            e.monsterId = "M01"; // 석상 보병
            e.target = target;
            e.moveSpeed = 2f; e.attackRange = 1.3f; e.attackDamage = 12f;
        }

        // ── 간단 디버그 HUD (정식 HUDManager는 Canvas 기반) ──
        void OnGUI()
        {
            if (_a == null) return;
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, 400, 20), $"A HP: {_a.CurrentHp:0}/{_a.MaxHp:0}   상태: {(_merge.IsMerged ? "합체" : "분리")}");
            if (_b != null)
            {
                string bState = _b.State == DownState.Down ? $"다운({_b.DownRemaining:0.0}s)" : _b.State.ToString();
                GUI.Label(new Rect(10, 30, 400, 20), $"B HP: {_b.CurrentHp:0}/{_b.MaxHp:0}  마나:{_b.Mana:0}  {bState}");
            }
            int bond = BondSystem.Instance != null ? BondSystem.Instance.Points : 0;
            GUI.Label(new Rect(10, 50, 400, 20), $"유대: {bond}p (등급 {(BondSystem.Instance != null ? BondSystem.Instance.Tier + 1 : 1)})");
            GUI.Label(new Rect(10, 75, 600, 20), "이동 ←→ / 점프 Space / 대쉬 LCtrl / 공격 좌클릭 / 방어 우클릭");
            GUI.Label(new Rect(10, 92, 600, 20), "A스킬 1·2 / B스킬 Q·E / 합체·분리 LShift / 조작전환 Tab / 던지기 G");
        }
    }
}
