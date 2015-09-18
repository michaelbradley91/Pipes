using Pipes.Models;
using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface ITwoOutletRandomisedPipeBuilder<TMessageType>
    {
        ITwoOutletPipe<TMessageType> WithLeftProbability(double probability);
        ITwoOutletPipe<TMessageType> WithRightProbability(double probability);
    }

    public class TwoOutletRandomisedPipeBuilder<TMessageType> : ITwoOutletRandomisedPipeBuilder<TMessageType>
    {
        internal TwoOutletRandomisedPipeBuilder()
        {
        }

        public ITwoOutletPipe<TMessageType> WithLeftProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return TwoOutletPipe<TMessageType>.CreateRandomised(probability);
        }

        public ITwoOutletPipe<TMessageType> WithRightProbability(double probability)
        {
            return WithLeftProbability(1 - probability);
        }
    }
}