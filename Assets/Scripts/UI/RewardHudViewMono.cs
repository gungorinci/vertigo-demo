using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VertigoDemo.Core;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class RewardHudViewMono : MonoBehaviour
    {
        private const string CONTAINER_NAME = "ui_container_rewards";
        private const string SCROLL_NAME = "ui_scroll_rewards";

        [SerializeField, FormerlySerializedAs("boxPrefab")] private RewardBoxViewMono _boxPrefab;
        [SerializeField, HideInInspector, FormerlySerializedAs("container")] private RectTransform _container;
        [SerializeField, HideInInspector, FormerlySerializedAs("scroll")] private ScrollRect _scroll;
        [SerializeField, Min(0f), FormerlySerializedAs("revealDuration")] private float _revealDuration = 0.3f;

        private readonly Dictionary<RewardData, RewardBoxViewMono> _boxes = new Dictionary<RewardData, RewardBoxViewMono>();
        private IEventBus _bus;
        private ObjectPool<RewardBoxViewMono> _boxPool;

        private void Awake()
        {
            _boxPool = new ObjectPool<RewardBoxViewMono>(CreateBox, OnGetBox, OnReleaseBox);
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
            if (!_boxes.TryGetValue(reward, out RewardBoxViewMono box))
            {
                box = _boxPool.Get();
                _boxes.Add(reward, box);
            }
            box.Show(reward.Icon, total);
        }

        private void Clear()
        {
            foreach (RewardBoxViewMono box in _boxes.Values)
            {
                _boxPool.Release(box);
            }
            _boxes.Clear();
        }

        #region Pool

        private RewardBoxViewMono CreateBox()
        {
            RewardBoxViewMono box = Instantiate(_boxPrefab, _container);
            box.name = _boxPrefab.name;
            return box;
        }

        private void OnGetBox(RewardBoxViewMono box)
        {
            box.gameObject.SetActive(true);
            box.transform.SetAsLastSibling();
        }

        private void OnReleaseBox(RewardBoxViewMono box)
        {
            box.gameObject.SetActive(false);
        }

        #endregion

        public void Reveal(RewardData reward)
        {
            float target = 1f;
            RewardBoxViewMono box;
            if (_boxes.TryGetValue(reward, out box))
            {
                target = NormalizedPositionOf((RectTransform)box.transform);
            }

            _scroll.DOKill();
            _scroll.DOHorizontalNormalizedPos(target, _revealDuration).SetLink(gameObject);
        }

        private float NormalizedPositionOf(RectTransform box)
        {
            float viewportWidth = _scroll.viewport.rect.width;
            float hiddenWidth = _container.rect.width - viewportWidth;
            if (hiddenWidth <= 0f)
            {
                return 0f;
            }
            float leftEdge = box.anchoredPosition.x - viewportWidth * 0.5f;
            return Mathf.Clamp01(leftEdge / hiddenWidth);
        }

        public Vector3 GetTargetPosition(RewardData reward)
        {
            if (_boxes.TryGetValue(reward, out RewardBoxViewMono box)) return box.transform.position;

            // A new box is appended at the right end of the container.
            var layout = _container.GetComponent<HorizontalLayoutGroup>();
            float spacing = layout && _boxes.Count > 0 ? layout.spacing : 0f;
            float halfBox = ((RectTransform)_boxPrefab.transform).rect.width * 0.5f;
            float rightPadding = layout ? layout.padding.right : 0f;
            float lastBoxRight = _container.rect.xMax - rightPadding;
            var local = new Vector3(lastBoxRight + spacing + halfBox, _container.rect.center.y);
            return _container.TransformPoint(local);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _container = transform.FindDeepComponent<RectTransform>(CONTAINER_NAME);
            _scroll = transform.FindDeepComponent<ScrollRect>(SCROLL_NAME);
        }
#endif
    }
}