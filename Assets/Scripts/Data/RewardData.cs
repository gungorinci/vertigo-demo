using UnityEngine;
using UnityEngine.Serialization;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Reward_", menuName = "Vertigo Demo/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("displayName")] private string _displayName;
        [SerializeField, FormerlySerializedAs("icon")] private Sprite _icon;
        [SerializeField, FormerlySerializedAs("isBomb")] private bool _isBomb;
        [SerializeField] private bool _isCurrency;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public bool IsBomb => _isBomb;

        public bool IsCurrency
        {
            get { return _isCurrency; }
        }
    }
}
