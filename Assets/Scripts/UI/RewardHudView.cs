using System.Collections.Generic;
using UnityEngine;
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            container = transform.FindDeepComponent<RectTransform>(ContainerName);
        }
#endif
    }
}