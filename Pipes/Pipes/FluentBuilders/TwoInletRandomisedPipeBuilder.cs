using System;
using Pipes.Models;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletRandomisedPipeBuilder
    {
        ITwoInletPipe WithLeftProbability(double probability);
        ITwoInletPipe WithRightProbability(double probability);
    }

    public class TwoInletRandomisedPipeBuilder : ITwoInletRandomisedPipeBuilder
    {
        internal TwoInletRandomisedPipeBuilder() { }

        public ITwoInletPipe WithLeftProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return new TwoInletPipe();
        }

        public ITwoInletPipe WithRightProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return new TwoInletPipe();
        }
    }
}
