using System;
using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public static class WeightedPicker
    {
        public static int Pick(IReadOnlyList<int> weights, IRandomSource random)
        {
            int total = 0;
            foreach (int weight in weights)
            {
                if (weight < 0) throw new ArgumentException("Weights cannot be negative.");
                total += weight;
            }

            if (total == 0) throw new ArgumentException("At least one weight must be positive.");

            int roll = random.Range(0, total);
            for (int i = 0; i < weights.Count; i++)
            {
                if (roll < weights[i]) return i;
                roll -= weights[i];
            }

            throw new InvalidOperationException("Unreachable: roll exceeded total weight.");
        }
    }
}