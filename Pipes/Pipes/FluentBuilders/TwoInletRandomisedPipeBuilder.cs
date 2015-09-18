using Pipes.Models;
using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletRandomisedPipeBuilder<TMessageType>
    {
        ITwoInletPipe<TMessageType> WithLeftProbability(double probability);
        ITwoInletPipe<TMessageType> WithRightProbability(double probability);
    }

    public class TwoInletRandomisedPipeBuilder<TMessageType> : ITwoInletRandomisedPipeBuilder<TMessageType>
    {
        internal TwoInletRandomisedPipeBuilder()
        {
        }

        public ITwoInletPipe<TMessageType> WithLeftProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return TwoInletPipe<TMessageType>.CreateRandomised(probability);
        }

        public ITwoInletPipe<TMessageType> WithRightProbability(double probability)
        {
            return WithLeftProbability(1 - probability);
        }
    }
}