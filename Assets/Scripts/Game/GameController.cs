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
            resultPanel.RestartClicked += OnRestartClicked;
        }

        private void OnDisable()
        {
            spinButton.onClick.RemoveListener(OnSpinClicked);
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
            resultPanel.RestartClicked -= OnRestartClicked;
        }

        private void Start()
        {
            wheels = new WheelSelector<WheelConfig>(normalWheel, safeWheel, superWheel);
            session = new GameSession<RewardData>();
            session.ZoneChanged += OnZoneChanged;
            session.RewardChanged += OnRewardChanged;
            session.StateChanged += OnStateChanged;
            session.RewardsCleared += OnRewardsCleared;
            LoadInventory();
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
            inventory = new Inventory<RewardData>();
            inventory.AddAll(InventorySave.Read(saveStore.Load(), catalog));
            inventory.Changed += OnInventoryChanged;
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

        private void OnInventoryChanged()
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

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= OnInventoryChanged;
            }
            if (session == null) return;
            session.ZoneChanged -= OnZoneChanged;
            session.RewardChanged -= OnRewardChanged;
            session.StateChanged -= OnStateChanged;
            session.RewardsCleared -= OnRewardsCleared;
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

        private void OnZoneChanged(int zone) => ShowZone();

        private void OnRewardChanged(RewardData reward, int total) => rewardHud.SetReward(reward, total);

        private void OnRewardsCleared() => rewardHud.Clear();

        private void OnStateChanged(SessionState state)
        {
            UpdateButtons();

            switch (state)
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

        private void OnRestartClicked()
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