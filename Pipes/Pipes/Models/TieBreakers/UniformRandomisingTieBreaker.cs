using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipes.Models.TieBreakers
{
    public interface IUniformRandomisingTieBreaker : ITieBreaker
    {
    }

    public class UniformRandomisingTieBreaker : IMinPrioritisingTieBreaker
    {
        private readonly Random randomNumberGenerator;

        public UniformRandomisingTieBreaker()
        {
            randomNumberGenerator = new Random();
        }

        public int ResolveTie(IEnumerable<int> tiedCompetitors)
        {
            var tiedCompetitorsList = tiedCompetitors.ToList();
            var resultIndex = randomNumberGenerator.Next(0, tiedCompetitorsList.Count - 1);
            return tiedCompetitorsList[resultIndex];
        }
    }
}
