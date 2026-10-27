using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameController : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_spin_wheel";
        private const string LeaveButtonName = "ui_button_leave";

        [SerializeField] private WheelConfig normalWheel;   // bronze, with the bomb
        [SerializeField] private WheelConfig safeWheel;     // silver, every 5th zone
        [SerializeField] private WheelConfig superWheel;    // gold, every 30th zone

        [Header("Result panel")]
        [SerializeField] private Sprite loseIcon;
        [SerializeField] private Sprite winIcon;
        [SerializeField, Min(0f)] private float resultDelay = 1f;
        [SerializeField, Min(0f)] private float flyDelay = 0.4f;

        [SerializeField, HideInInspector] private Button spinButton;
        [SerializeField, HideInInspector] private Button leaveButton;
        [SerializeField, HideInInspector] private WheelView wheelView;
        [SerializeField, HideInInspector] private WheelSpinAnimator spinAnimator;
        [SerializeField, HideInInspector] private ResultPanelView resultPanel;
        [SerializeField, HideInInspector] private HudView hud;
        [SerializeField, HideInInspector] private RewardHudView rewardHud;
        [SerializeField, HideInInspector] private WheelWinEffect winEffect;
        [SerializeField, HideInInspector] private RewardFlyEffect rewardFly;
        [SerializeField, HideInInspector] private WalletView walletView;

        private readonly IRandomSource random = new SystemRandomSource();
        private readonly IEventBus bus = new EventBus();
        private List<RolledSlice> currentSlices;
        private GameSession<RewardData> session;
        private WheelConfig currentConfig;
        private WheelSelector<WheelConfig> wheels;
        private bool showIntro = true;
        private IInventory<RewardData> inventory;
        private ISaveStore saveStore;
        private readonly List<RewardData> currencies = new List<RewardData>();

        private void OnEnable()
        {
            spinButton.onClick.AddListener(OnSpinClicked);
            leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDisable()
        {
            spinButton.onClick.RemoveListener(OnSpinClicked);
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        private void Start()
        {
            wheels = new WheelSelector<WheelConfig>(normalWheel, safeWheel, superWheel);
            session = new GameSession<RewardData>(bus);
            resultPanel.Bind(bus);
            LoadInventory();
            Subscribe();
            ShowZone();
            UpdateButtons();
        }

        private void LoadInventory()
        {
            List<RewardData> catalog = CollectRewards();
            foreach (RewardData reward in catalog)
            {
                if (reward.IsCurrency)
                {
                    currencies.Add(reward);
                }
            }

            saveStore = new FileSaveStore();
            inventory = new Inventory<RewardData>(bus);
            inventory.AddAll(InventorySave.Read(saveStore.Load(), catalog));
            ShowWallet();
        }

        private List<RewardData> CollectRewards()
        {
            var rewards = new List<RewardData>();
            foreach (WheelConfig wheel in new[] { normalWheel, safeWheel, superWheel })
            {
                foreach (WheelSlice slice in wheel.Slices)
                {
                    foreach (RewardData reward in slice.RewardPool)
                    {
                        if (reward && !rewards.Contains(reward))
                        {
                            rewards.Add(reward);
                        }
                    }
                }
            }
            return rewards;
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            saveStore.Save(InventorySave.Write(inventory.Items));
            ShowWallet();
        }

        private void ShowWallet()
        {
            foreach (RewardData currency in currencies)
            {
                walletView.Show(currency, inventory.GetAmount(currency));
            }
        }

        private void Subscribe()
        {
            bus.Subscribe<ZoneChangedEvent>(OnZoneChanged);
            bus.Subscribe<RewardChangedEvent<RewardData>>(OnRewardChanged);
            bus.Subscribe<RewardsClearedEvent>(OnRewardsCleared);
            bus.Subscribe<SessionStateChangedEvent>(OnStateChanged);
            bus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            bus.Subscribe<RestartRequestedEvent>(OnRestartRequested);
        }

        private void OnDestroy()
        {
            bus.Unsubscribe<ZoneChangedEvent>(OnZoneChanged);
            bus.Unsubscribe<RewardChangedEvent<RewardData>>(OnRewardChanged);
            bus.Unsubscribe<RewardsClearedEvent>(OnRewardsCleared);
            bus.Unsubscribe<SessionStateChangedEvent>(OnStateChanged);
            bus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            bus.Unsubscribe<RestartRequestedEvent>(OnRestartRequested);
        }

        private void ShowZone()
        {
            winEffect.Hide();
            currentConfig = wheels.For(session.ZoneType);
            currentSlices = WheelRoller.Roll(currentConfig, session.Zone, random);
            wheelView.Show(currentConfig, currentSlices);
            if (showIntro) hud.ShowIntro(session.Zone);
            else hud.ShowZone(session.Zone, session.ZoneType);
        }

        private void OnSpinClicked()
        {
            if (!session.CanSpin) return;

            var weights = new List<int>(currentSlices.Count);
            int totalWeight = 0;
            foreach (RolledSlice slice in currentSlices)
            {
                weights.Add(slice.Weight);
                totalWeight += slice.Weight;
            }

            if (totalWeight == 0)
            {
                Debug.LogError($"{name}: no slice can be picked on '{currentConfig.name}'.", currentConfig);
                return;
            }

            int winner = WeightedPicker.Pick(weights, random);
            session.BeginSpin();
            if (showIntro)
            {
                showIntro = false;
                hud.ShowZone(session.Zone, session.ZoneType);
            }
            spinAnimator.Spin(winner, currentSlices.Count, random, () => OnSpinFinished(winner));
        }

        private void OnSpinFinished(int winner)
        {
            RolledSlice slice = currentSlices[winner];
            winEffect.Play(winner);

            if (slice.Reward.IsBomb)
            {
                DOVirtual.DelayedCall(resultDelay, () => CompleteSpin(slice)).SetLink(gameObject);
                return;
            }

            rewardHud.Reveal(slice.Reward);
            DOVirtual.DelayedCall(flyDelay, () => rewardFly.Fly(
                    slice.Reward.Icon,
                    wheelView.GetSliceIcon(winner),
                    rewardHud.GetTargetPosition(slice.Reward),
                    () => CompleteSpin(slice)))
                .SetLink(gameObject);
        }

        private void CompleteSpin(RolledSlice slice) =>
            session.CompleteSpin(slice.Reward, slice.Amount, slice.Reward.IsBomb);

        private void OnZoneChanged(ZoneChangedEvent evt) => ShowZone();

        private void OnRewardChanged(RewardChangedEvent<RewardData> evt) => rewardHud.SetReward(evt.Reward, evt.Total);

        private void OnRewardsCleared(RewardsClearedEvent evt) => rewardHud.Clear();

        private void OnStateChanged(SessionStateChangedEvent evt)
        {
            UpdateButtons();

            switch (evt.State)
            {
                case SessionState.Lost:
                    resultPanel.Show(loseIcon, "BOMB! ALL REWARDS LOST", "RESTART");
                    break;
                case SessionState.Left:
                    resultPanel.Show(winIcon, "REWARDS COLLECTED!", "PLAY AGAIN");
                    break;
                case SessionState.Idle:
                    resultPanel.Hide();
                    break;
            }
        }

        private void UpdateButtons()
        {
            spinButton.interactable = session.CanSpin;
            leaveButton.interactable = session.CanLeave;
        }

        private void OnLeaveClicked()
        {
            if (!session.CanLeave) return;

            inventory.AddAll(session.Collected);
            session.Leave();
        }

        private void OnRestartRequested(RestartRequestedEvent evt)
        {
            showIntro = true;
            session.Restart();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Transform button = transform.FindDeep(SpinButtonName);
            spinButton = button != null ? button.GetComponent<Button>() : null;
            wheelView = GetComponentInChildren<WheelView>(true);
            spinAnimator = GetComponentInChildren<WheelSpinAnimator>(true);

            Transform leave = transform.FindDeep(LeaveButtonName);
            leaveButton = leave != null ? leave.GetComponent<Button>() : null;
            resultPanel = GetComponentInChildren<ResultPanelView>(true);
            hud = GetComponentInChildren<HudView>(true);
            rewardHud = GetComponentInChildren<RewardHudView>(true);
            winEffect = GetComponentInChildren<WheelWinEffect>(true);
            rewardFly = GetComponentInChildren<RewardFlyEffect>(true);
            walletView = GetComponentInChildren<WalletView>(true);

            if (spinButton == null || leaveButton == null || wheelView == null || spinAnimator == null
                || resultPanel == null || hud == null || rewardHud == null || winEffect == null || rewardFly == null
                || walletView == null)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}