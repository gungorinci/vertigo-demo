using UnityEngine;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Reward_", menuName = "Vertigo Demo/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isBomb;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public bool IsBomb => isBomb;
    }
}
