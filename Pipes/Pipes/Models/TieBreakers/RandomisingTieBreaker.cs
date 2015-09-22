using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IRandomisingTieBreaker : ITieBreaker
    {
        double LeftProbability { get; }
        double RightProbability { get; }
    }

    public class RandomisingTieBreaker : IRandomisingTieBreaker
    {
        public double LeftProbability { get; private set; }
        public double RightProbability { get { return 1 - LeftProbability; } }

        private readonly Random randomNumberGenerator;

        public RandomisingTieBreaker(double leftProbability)
        {
            LeftProbability = leftProbability;
            randomNumberGenerator = new Random();
        }

        public TieResult ResolveTie()
        {
            var result = randomNumberGenerator.NextDouble();
            // 1.0 is not a possible result of next double, but 0.0 is.
            return result < LeftProbability ? TieResult.Left : TieResult.Right;
        }
    }
}
