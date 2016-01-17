using System.Collections.Generic;
using System.Linq;

namespace Pipes.Models.TieBreakers
{
    public interface IMinPrioritisingTieBreaker : ITieBreaker
    {
    }

    public class MinPrioritisingTieBreaker : IMinPrioritisingTieBreaker
    {
        public int ResolveTie(IEnumerable<int> tiedCompetitors)
        {
            return tiedCompetitors.Min();
        }
    }
}
