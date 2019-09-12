using System;
using System.Collections.Generic;

namespace TFABot
{

    public sealed class clsRollingAverage
    {
        public clsRollingAverage(int maxRememberedNumbers)
        {
            if (maxRememberedNumbers <= 0)
            {
                throw new ArgumentException("maxRememberedNumbersmust be greater than 0.", "maxRememberedNumbers");
            }

            counts = new Queue<int>(maxRememberedNumbers);
            maxSize = maxRememberedNumbers;
            currentTotal = 0L;
            padLock = new object();
        }

        private const int defaultMaxRememberedNumbers = 10;

        private readonly Queue<int> counts;
        private readonly int maxSize;

        private long currentTotal;

        private object padLock;

        public void Add(int value)
        {
            lock (padLock)
            {
                if (value < LowestEver) LowestEver = value;
                if (value > HighestEver) HighestEver = value;

                if (counts.Count == maxSize)
                {
                    currentTotal -= counts.Dequeue();
                }

                counts.Enqueue(value);

                currentTotal += value;
            }
        }


        public int LowestEver { get; private set; }
        public int HighestEver { get; private set; }

        public int CurrentAverage
        {
            get
            {
                long lenCounts;
                long observedTotal;

                lock (padLock)
                {
                    lenCounts = counts.Count;
                    observedTotal = currentTotal;
                }

                if (lenCounts == 0)
                {
                    return LowestEver = 0;
                }
                else if (lenCounts == 1)
                {
                    return (int)observedTotal;
                }
                else
                {
                    return (int)(observedTotal / lenCounts);
                }
            }
        }

        public void Clear()
        {
            lock (padLock)
            {
                currentTotal = 0L;
                counts.Clear();
            }
        }

        public int Count
        {
            get
            {
                return counts.Count;
            }
        }

        public int[] GetValues()
        {
            return counts.ToArray();
        }
    }
}