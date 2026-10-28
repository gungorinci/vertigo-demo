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

        public string DisplayName
        {
            get { return _displayName; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public bool IsBomb
        {
            get { return _isBomb; }
        }

        public bool IsCurrency
        {
            get { return _isCurrency; }
        }
    }
}
