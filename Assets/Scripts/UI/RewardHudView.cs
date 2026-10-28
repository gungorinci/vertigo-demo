using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VertigoDemo.Core;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class RewardHudView : MonoBehaviour
    {
        private const string ContainerName = "ui_container_rewards";
        private const string ScrollName = "ui_scroll_rewards";

        [SerializeField] private RewardBoxView boxPrefab;
        [SerializeField, HideInInspector] private RectTransform container;
        [SerializeField, HideInInspector] private ScrollRect scroll;
        [SerializeField, Min(0f)] private float revealDuration = 0.3f;

        private readonly Dictionary<RewardData, RewardBoxView> boxes = new Dictionary<RewardData, RewardBoxView>();
        private IEventBus _bus;
        private ObjectPool<RewardBoxView> _boxPool;

        private void Awake()
        {
            _boxPool = new ObjectPool<RewardBoxView>(CreateBox, OnGetBox, OnReleaseBox);
        }

        public void Bind(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<RewardChangedEvent<RewardData>>(OnRewardChanged);
            _bus.Subscribe<RewardsClearedEvent>(OnRewardsCleared);
        }

        private void OnDestroy()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<RewardChangedEvent<RewardData>>(OnRewardChanged);
                _bus.Unsubscribe<RewardsClearedEvent>(OnRewardsCleared);
            }
        }

        private void OnRewardChanged(RewardChangedEvent<RewardData> evt)
        {
            SetReward(evt.Reward, evt.Total);
        }

        private void OnRewardsCleared(RewardsClearedEvent evt)
        {
            Clear();
        }

        private void SetReward(RewardData reward, int total)
        {
            if (!boxes.TryGetValue(reward, out RewardBoxView box))
            {
                box = _boxPool.Get();
                boxes.Add(reward, box);
            }
            box.Show(reward.Icon, total);
        }

        private void Clear()
        {
            foreach (RewardBoxView box in boxes.Values)
            {
                _boxPool.Release(box);
            }
            boxes.Clear();
        }

        #region Pool

        private RewardBoxView CreateBox()
        {
            RewardBoxView box = Instantiate(boxPrefab, container);
            box.name = boxPrefab.name;
            return box;
        }

        private void OnGetBox(RewardBoxView box)
        {
            box.gameObject.SetActive(true);
            box.transform.SetAsLastSibling();
        }

        private void OnReleaseBox(RewardBoxView box)
        {
            box.gameObject.SetActive(false);
        }

        #endregion

        public void Reveal(RewardData reward)
        {
            float target = 1f;
            RewardBoxView box;
            if (boxes.TryGetValue(reward, out box))
            {
                target = NormalizedPositionOf((RectTransform)box.transform);
            }

            scroll.DOKill();
            scroll.DOHorizontalNormalizedPos(target, revealDuration).SetLink(gameObject);
        }

        private float NormalizedPositionOf(RectTransform box)
        {
            float viewportWidth = scroll.viewport.rect.width;
            float hiddenWidth = container.rect.width - viewportWidth;
            if (hiddenWidth <= 0f)
            {
                return 0f;
            }
            float leftEdge = box.anchoredPosition.x - viewportWidth * 0.5f;
            return Mathf.Clamp01(leftEdge / hiddenWidth);
        }

        public Vector3 GetTargetPosition(RewardData reward)
        {
            if (boxes.TryGetValue(reward, out RewardBoxView box)) return box.transform.position;

            // A new box is appended at the right end of the container.
            var layout = container.GetComponent<HorizontalLayoutGroup>();
            float spacing = layout && boxes.Count > 0 ? layout.spacing : 0f;
            float halfBox = ((RectTransform)boxPrefab.transform).rect.width * 0.5f;
            float rightPadding = layout ? layout.padding.right : 0f;
            float lastBoxRight = container.rect.xMax - rightPadding;
            var local = new Vector3(lastBoxRight + spacing + halfBox, container.rect.center.y);
            return container.TransformPoint(local);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            container = transform.FindDeepComponent<RectTransform>(ContainerName);
            scroll = transform.FindDeepComponent<ScrollRect>(ScrollName);
        }
#endif
    }
}