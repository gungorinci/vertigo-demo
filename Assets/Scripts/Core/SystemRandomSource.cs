using System;

namespace VertigoDemo.Core
{
    public class SystemRandomSource : IRandomSource
    {
        private readonly Random random;

        public SystemRandomSource() : this(Environment.TickCount) { }

        public SystemRandomSource(int seed)
        {
            random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive) =>
            random.Next(minInclusive, maxExclusive);
    }
}