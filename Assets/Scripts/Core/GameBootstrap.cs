// GameBootstrap.cs — Boot 씬 초기화 + 시스템 와이어링 (문서 06-5 씬 구조).
// Boot → Title → Hub → Loading → Dungeon. 인스펙터에서 참조 연결 후 사용.
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Characters;
using BroDungeon.Combat;
using BroDungeon.Dungeon;
using BroDungeon.Items;
using BroDungeon.Network;

namespace BroDungeon.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("매니저")]
        public GameManager gameManager;
        public SaveManager saveManager;
        public AudioManager audioManager;
        public NetworkManager network;
        public InputManager input;

        [Header("캐릭터/시스템")]
        public WarriorController warrior;
        public MageController mage;
        public MergeSystem merge;
        public ThrowSystem throwSystem;
        public ParrySystem parry;
        public BondSystem bond;
        public CombatManager combat;
        public SkillSystem warriorSkills, mageSkills;

        [Header("던전")]
        public RoomManager rooms;
        public DungeonGenerator generator;
        public WeatherSystem weather;

        [Header("아이템/제단/런")]
        public EquipmentSystem equipment;
        public CursedItemHandler cursedHandler;
        public AltarSystem altar;
        public RunManager runManager;

        void Start()
        {
            WireReferences();
            LoadOrStartRun();
        }

        void WireReferences()
        {
            if (warrior) warrior.GameManagerRef = gameManager;
            if (mage) mage.GameManagerRef = gameManager;

            if (merge) { merge.A = warrior; merge.B = mage; }
            if (throwSystem) { throwSystem.A = warrior; throwSystem.B = mage; throwSystem.merge = merge; }
            if (parry) { parry.merge = merge; if (warrior) warrior.parry = parry; if (mage) mage.parry = parry; }

            if (input)
            {
                input.warrior = warrior; input.mage = mage; input.merge = merge;
                input.throwSystem = throwSystem; input.parry = parry;
                input.warriorSkills = warriorSkills; input.mageSkills = mageSkills;
            }

            if (warriorSkills) warriorSkills.owner = warrior;
            if (mageSkills) mageSkills.owner = mage;

            if (rooms) { rooms.generator = generator; rooms.weather = weather; }
            if (weather) { weather.equipment = equipment; weather.warrior = warrior; weather.mage = mage; }

            // 아이템/제단/저주 와이어링
            if (equipment)
            {
                equipment.warrior = warrior; equipment.mage = mage;
                equipment.warriorSkills = warriorSkills; equipment.mageSkills = mageSkills;
                equipment.cursedHandler = cursedHandler;
            }
            if (cursedHandler)
            {
                cursedHandler.a = warrior; cursedHandler.b = mage; cursedHandler.merge = merge;
                cursedHandler.equipment = equipment; cursedHandler.loot = LootSystem.Instance;
                cursedHandler.EnsureRegistered();
            }
            if (altar) altar.equipment = equipment;

            if (runManager)
            {
                runManager.equipment = equipment; runManager.altar = altar;
                runManager.bond = bond; runManager.rooms = rooms;
            }
        }

        void LoadOrStartRun()
        {
            int seed = 0;
            if (network != null) { network.StartSingle(); seed = network.RunSeed; }

            // 중단 세이브가 있으면 이어하기(문서 06-7)
            if (saveManager != null && saveManager.HasRunSave())
            {
                var run = saveManager.LoadRunAndConsume();
                seed = run != null ? run.seed : seed;
            }

            // 런 시작(런버프 초기화 → 영구강화 재적용 → 유대 로드 → 던전 시작)
            if (runManager != null) runManager.StartRun(GameMode.Story, seed);
            else { gameManager.Mode = GameMode.Story; rooms?.StartRun(seed); }

            audioManager?.PlayBGM("04"); // 1구역 탐험 BGM
        }
    }
}
