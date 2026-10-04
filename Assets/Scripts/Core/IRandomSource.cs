namespace VertigoDemo.Core
{
    public interface IRandomSource
    {
        /// <summary>Returns a random int in [minInclusive, maxExclusive).</summary>
        int Range(int minInclusive, int maxExclusive);
    }
}