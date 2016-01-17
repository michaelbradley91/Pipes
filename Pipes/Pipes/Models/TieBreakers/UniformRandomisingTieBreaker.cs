using System;

namespace Pipes.Models.TieBreakers
{
    public interface IUniformRandomisingTieBreaker : ITieBreaker
    {
    }

    public class UniformRandomisingTieBreaker : TieBreaker, IUniformRandomisingTieBreaker
    {
        private readonly Random randomNumberGenerator;

        public UniformRandomisingTieBreaker()
        {
            randomNumberGenerator = new Random();
        }

        protected override int ResolveTie(int[] tiedCompetitors)
        {
            var resultIndex = randomNumberGenerator.Next(0, tiedCompetitors.Length);
            return tiedCompetitors[resultIndex];
        }
    }
}
