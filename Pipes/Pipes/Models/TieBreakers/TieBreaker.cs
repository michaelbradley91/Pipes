using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipes.Models.TieBreakers
{
    public interface ITieBreaker
    {
        /// <summary>
        /// Return the competitor who won by resolving the tie. Each competitor is represented by an integer, and the same competitor should always
        /// be represented by the same integer when passed in. Note that there can be a varying number of competitors tied.
        /// 
        /// You will receive an invalid operation exception if a competitor is passed in more than once, or if there are zero competitors.
        /// 
        /// The integer returned is the competitor who was won the tie.
        /// </summary>
        int ResolveTie(IEnumerable<int> tiedCompetitors);
    }

    public abstract class TieBreaker : ITieBreaker
    {
        public int ResolveTie(IEnumerable<int> tiedCompetitors)
        {
            var tiedCompetitorsList = tiedCompetitors.ToArray();
            if (tiedCompetitorsList.Length == 0) throw new ArgumentException("There must be at least one competitor to resolve a tie.", nameof(tiedCompetitors));
            if (tiedCompetitorsList.Length == 1) return tiedCompetitorsList[0];
            if (tiedCompetitorsList.Distinct().Count() != tiedCompetitorsList.Length) throw new ArgumentException("All competitors in a tie should be unique.", nameof(tiedCompetitors));
            
            return ResolveTie(tiedCompetitorsList);
        }

        /// <summary>
        /// Resolve the tie between the given competitors. All competitors are guaranteed to be unique and there will be at least two.
        /// </summary>
        protected abstract int ResolveTie(int[] tiedCompetitors);
    }
}
