using System;

namespace VertigoDemo.Core
{
    public class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource() : this(Environment.TickCount) { }

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive) =>
            _random.Next(minInclusive, maxExclusive);
    }
}