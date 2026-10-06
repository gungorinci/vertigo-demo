using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class RewardHudView : MonoBehaviour
    {
        private const string ContainerName = "ui_container_rewards";

        [SerializeField] private RewardBoxView boxPrefab;
        [SerializeField, HideInInspector] private RectTransform container;

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

        public Vector3 GetTargetPosition(RewardData reward)
        {
            if (boxes.TryGetValue(reward, out RewardBoxView box)) return box.transform.position;

            // A new box is appended at the right end of the container.
            var layout = container.GetComponent<HorizontalLayoutGroup>();
            float spacing = layout != null && boxes.Count > 0 ? layout.spacing : 0f;
            float halfBox = ((RectTransform)boxPrefab.transform).rect.width * 0.5f;
            var local = new Vector3(container.rect.xMax + spacing + halfBox, container.rect.center.y);
            return container.TransformPoint(local);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            container = transform.FindDeepComponent<RectTransform>(ContainerName);
        }
#endif
    }
}