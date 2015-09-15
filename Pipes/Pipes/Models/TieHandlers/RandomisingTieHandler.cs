using System;
using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    internal class RandomisingTieHandler : ITieHandler
    {
        private readonly double leftProbability;
        private readonly Random randomNumberGenerator;

        public RandomisingTieHandler(double leftProbability)
        {
            this.leftProbability = leftProbability;
            randomNumberGenerator = new Random();
        }

        public TieResult ResolveTie()
        {
            var result = randomNumberGenerator.NextDouble();
            // 1.0 is not a possible result of next double, but 0.0 is.
            return result < leftProbability ? TieResult.Left : TieResult.Right;
        }
    }
}
