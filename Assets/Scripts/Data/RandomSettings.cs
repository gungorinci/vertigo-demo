using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "RandomSettings", menuName = "Vertigo Demo/Random Settings")]
    public class RandomSettings : ScriptableObject
    {
        private const int NO_SEED = 0;

        [SerializeField, Tooltip("0 picks a new seed every run. Any other value replays the same spins.")]
        private int _seed = NO_SEED;

        #region Public API

        public IRandomSource CreateSource()
        {
            if (_seed == NO_SEED)
            {
                return new SystemRandomSource();
            }
            return new SystemRandomSource(_seed);
        }

        #endregion
    }
}
