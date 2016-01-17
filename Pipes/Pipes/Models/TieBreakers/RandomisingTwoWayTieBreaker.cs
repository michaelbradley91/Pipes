using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IRandomisingTwoWayTieBreaker : ITwoWayTieBreaker
    {
        double LeftProbability { get; }
        double RightProbability { get; }
    }

    public class RandomisingTwoWayTieBreaker : TwoWayTieBreaker, IRandomisingTwoWayTieBreaker
    {
        public double LeftProbability { get; }
        public double RightProbability => 1 - LeftProbability;

        private readonly Random randomNumberGenerator;

        public RandomisingTwoWayTieBreaker(double leftProbability)
        {
            LeftProbability = leftProbability;
            randomNumberGenerator = new Random();
        }

        public override TieResult ResolveTie()
        {
            var result = randomNumberGenerator.NextDouble();
            // 1.0 is not a possible result of next double, but 0.0 is.
            return result < LeftProbability ? TieResult.Left : TieResult.Right;
        }
    }
}
