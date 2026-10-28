namespace VertigoDemo.Core
{
    public static class ZoneRules
    {
        public static bool CanLeave(bool isSpinning, bool hasRewards)
        {
            return !isSpinning && hasRewards;
        }
    }
}
