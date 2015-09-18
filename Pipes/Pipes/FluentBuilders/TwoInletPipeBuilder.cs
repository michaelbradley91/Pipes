using Pipes.Constants;
using Pipes.Models;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletPipeBuilder<TMessage>
    {
        ITwoInletPipe<TMessage> Prioritised(Priority priority);
        ITwoInletRandomisedPipeBuilder<TMessage> Randomised();
        ITwoInletPipe<TMessage> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially);
    }

    public class TwoInletPipeBuilder<TMessage> : ITwoInletPipeBuilder<TMessage>
    {
        internal TwoInletPipeBuilder()
        {
        }

        public ITwoInletPipe<TMessage> Prioritised(Priority priority)
        {
            return TwoInletPipe<TMessage>.CreatePrioritised(priority);
        }

        public ITwoInletRandomisedPipeBuilder<TMessage> Randomised()
        {
            return new TwoInletRandomisedPipeBuilder<TMessage>();
        }

        public ITwoInletPipe<TMessage> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially)
        {
            return TwoInletPipe<TMessage>.CreateAlternated(alternated);
        }

        public ITwoInletPipe<TMessage> WithTieHandler(ITieBreaker tieBreaker)
        {
            return TwoInletPipe<TMessage>.Create(tieBreaker);
        }
    }
}