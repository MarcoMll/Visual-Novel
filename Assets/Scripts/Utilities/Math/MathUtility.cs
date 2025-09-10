using UnityEngine;

namespace VisualNovel.Utilities
{
    public static class MathUtility
    {
        /// <summary>
        /// Randomly distributes `total` into three integers (a, b, c) with a + b + c = total.
        /// Uniform over all non-negative integer triples. Set 'minPerBucket' to assign a minimum value per split.
        /// </summary>
        public static void SplitIntoThree(int total, out int a, out int b, out int c, int minPerBucket = 0)
        {
            if (total < 0) throw new System.ArgumentOutOfRangeException(nameof(total), "total must be >= 0");
            if (minPerBucket < 0) throw new System.ArgumentOutOfRangeException(nameof(minPerBucket), "minPerBucket must be >= 0");

            // Reserve the minimum requested per bucket, then distribute the rest.
            int N = total - 3 * minPerBucket;
            if (N < 0)
                throw new System.ArgumentException("total is too small for the requested minimum per bucket.", nameof(total));

            // Number of pairs (x,y) with 0 <= x <= y <= N
            long totalPairs = ((long)N + 1) * (N + 2) / 2;
            if (totalPairs > int.MaxValue)
                throw new System.ArgumentOutOfRangeException(nameof(total), "total too large for this implementation.");

            // Pick one pair uniformly.
            int u = Random.Range(0, (int)totalPairs); // int maxExclusive
            int x = 0;
            int remaining = u;

            // Find x block such that u falls within it (block size = N - x + 1).
            while (true)
            {
                int countForX = N - x + 1;
                if (remaining < countForX) break;
                remaining -= countForX;
                x++;
            }

            int y = x + remaining;

            // Map (x,y) -> (a,b,c): a = x, b = y - x, c = N - y
            a = x + minPerBucket;
            b = (y - x) + minPerBucket;
            c = (N - y) + minPerBucket;
        }
    }
}