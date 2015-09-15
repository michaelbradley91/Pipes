using Pipes.Models;
using System;

namespace Pipes.FluentBuilders
{
    public interface ITwoOutletRandomisedPipeBuilder
    {
        ITwoOutletPipe WithLeftProbability(double probability);
        ITwoOutletPipe WithRightProbability(double probability);
    }

    public class TwoOutletRandomisedPipeBuilder : ITwoOutletRandomisedPipeBuilder
    {
        internal TwoOutletRandomisedPipeBuilder()
        {
        }

        public ITwoOutletPipe WithLeftProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return new TwoOutletPipe();
        }

        public ITwoOutletPipe WithRightProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return new TwoOutletPipe();
        }
    }
}