namespace VertigoDemo.UI
{
    public static class AmountFormatter
    {
        private const string AMOUNT_FORMAT = "x{0}";

        #region Public API

        public static string Format(int amount)
        {
            return string.Format(AMOUNT_FORMAT, amount);
        }

        #endregion
    }
}
