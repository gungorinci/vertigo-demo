namespace VertigoDemo.Core
{
    public static class ZoneRules
    {
        #region Public API

        public static bool CanLeave(bool isSpinning, bool hasRewards)
        {
            return !isSpinning && hasRewards;
        }

        #endregion
    }
}
