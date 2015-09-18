using Pipes.Models;
using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletRandomisedPipeBuilder<TMessage>
    {
        ITwoInletPipe<TMessage> WithLeftProbability(double probability);
        ITwoInletPipe<TMessage> WithRightProbability(double probability);
    }

    public class TwoInletRandomisedPipeBuilder<TMessage> : ITwoInletRandomisedPipeBuilder<TMessage>
    {
        internal TwoInletRandomisedPipeBuilder()
        {
        }

        public ITwoInletPipe<TMessage> WithLeftProbability(double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("probability");
            return TwoInletPipe<TMessage>.CreateRandomised(probability);
        }

        public ITwoInletPipe<TMessage> WithRightProbability(double probability)
        {
            return WithLeftProbability(1 - probability);
        }
    }
}