using System;
using System.Linq;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface ITwoWayTieBreaker : ITieBreaker
    {
        TieResult ResolveTie();
    }

    public abstract class TwoWayTieBreaker : TieBreaker, ITwoWayTieBreaker
    {
        protected override int ResolveTie(int[] tiedCompetitors)
        {
            if (tiedCompetitors.Length != 2) throw new InvalidOperationException("A Two Way TieBreaker can only resolve ties between two competitors");
            var result = ResolveTie();
            switch (result)
            {
                case TieResult.Left:
                    return tiedCompetitors.Min();
                case TieResult.Right:
                    return tiedCompetitors.Max();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public abstract TieResult ResolveTie();
    }
}
