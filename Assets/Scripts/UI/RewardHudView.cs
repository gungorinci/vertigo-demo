using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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

        public void SetReward(RewardData reward, int total)
        {
            if (!boxes.TryGetValue(reward, out RewardBoxView box))
            {
                box = Instantiate(boxPrefab, container);
                box.name = boxPrefab.name;
                boxes.Add(reward, box);
            }
            box.Show(reward.Icon, total);
        }

        public void Clear()
        {
            foreach (RewardBoxView box in boxes.Values) Destroy(box.gameObject);
            boxes.Clear();
        }

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
            float spacing = layout != null && boxes.Count > 0 ? layout.spacing : 0f;
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