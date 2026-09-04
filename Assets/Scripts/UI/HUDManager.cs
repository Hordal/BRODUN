// HUDManager.cs — 인게임 HUD (문서 06-1). 이벤트 구독으로 갱신.
using UnityEngine;
using UnityEngine.UI;
using BroDungeon.Data;
using BroDungeon.Characters;
using BroDungeon.Utilities;

namespace BroDungeon.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("캐릭터 참조")]
        public WarriorController warrior;
        public MageController mage;

        [Header("바 (문서 06-1)")]
        public Image aHpBar;        // 좌상단, 초록→노랑→빨강
        public Image bHpBar;        // A 아래, 위험 시 깜빡
        public Image aStaminaBar;
        public Image bManaBar;

        [Header("유대/합체")]
        public Image bondIcon;
        public Text bondTierText;
        public Image mergeStateIcon;

        [Header("정보 바")]
        public Text goldText, sacrificeText, floorText;

        [Header("다운 표시 (I-06)")]
        public GameObject downWarning;
        public Text downTimerText;

        void OnEnable()
        {
            EventBus.Subscribe<BondChangedEvent>(OnBond);
            EventBus.Subscribe<MergeStateChangedEvent>(OnMerge);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrency);
            EventBus.Subscribe<CharacterDownedEvent>(OnDowned);
            EventBus.Subscribe<CharacterRevivedEvent>(OnRevived);
        }
        void OnDisable()
        {
            EventBus.Unsubscribe<BondChangedEvent>(OnBond);
            EventBus.Unsubscribe<MergeStateChangedEvent>(OnMerge);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrency);
            EventBus.Unsubscribe<CharacterDownedEvent>(OnDowned);
            EventBus.Unsubscribe<CharacterRevivedEvent>(OnRevived);
        }

        void Update()
        {
            if (warrior != null && aHpBar != null)
            {
                float r = warrior.CurrentHp / Mathf.Max(1f, warrior.MaxHp);
                aHpBar.fillAmount = r;
                aHpBar.color = r > 0.5f ? Color.green : r > 0.25f ? Color.yellow : Color.red;
                if (aStaminaBar) aStaminaBar.fillAmount = warrior.Stamina / Mathf.Max(1f, warrior.MaxStamina);
            }
            if (mage != null)
            {
                if (bHpBar) bHpBar.fillAmount = mage.CurrentHp / Mathf.Max(1f, mage.MaxHp);
                if (bManaBar) bManaBar.fillAmount = mage.Mana / Mathf.Max(1f, mage.MaxMana);
                if (mage.State == DownState.Down && downTimerText)
                    downTimerText.text = Mathf.CeilToInt(mage.DownRemaining).ToString();
            }
        }

        void OnBond(BondChangedEvent e) { if (bondTierText) bondTierText.text = $"{e.Tier + 1}등급"; }
        void OnMerge(MergeStateChangedEvent e)
        { if (mergeStateIcon) mergeStateIcon.color = e.State == MergeState.Merged ? Color.cyan : Color.magenta; }
        void OnCurrency(CurrencyChangedEvent e)
        {
            if (e.Type == Currency.Gold && goldText) goldText.text = e.NewAmount.ToString();
            if (e.Type == Currency.Sacrifice && sacrificeText) sacrificeText.text = e.NewAmount.ToString();
        }
        void OnDowned(CharacterDownedEvent e) { if (e.IsB && downWarning) downWarning.SetActive(true); }
        void OnRevived(CharacterRevivedEvent e) { if (e.IsB && downWarning) downWarning.SetActive(false); }
    }
}
