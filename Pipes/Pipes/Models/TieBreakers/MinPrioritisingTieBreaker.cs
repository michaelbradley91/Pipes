using System.Linq;

namespace Pipes.Models.TieBreakers
{
    public interface IMinPrioritisingTieBreaker : ITieBreaker
    {
    }

    public class MinPrioritisingTieBreaker : TieBreaker, IMinPrioritisingTieBreaker
    {
        protected override int ResolveTie(int[] tiedCompetitors)
        {
            return tiedCompetitors.Min();
        }
    }
}
