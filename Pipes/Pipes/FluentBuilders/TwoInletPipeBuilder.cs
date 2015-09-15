using Pipes.Constants;
using Pipes.Models;
using Pipes.Models.TieBreakers;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletPipeBuilder<TMessageType>
    {
        ITwoInletPipe<TMessageType> Prioritised(Priority priority);
        ITwoInletRandomisedPipeBuilder<TMessageType> Randomised();
        ITwoInletPipe<TMessageType> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially);
    }

    public class TwoInletPipeBuilder<TMessageType> : ITwoInletPipeBuilder<TMessageType>
    {
        internal TwoInletPipeBuilder()
        {
        }

        public ITwoInletPipe<TMessageType> Prioritised(Priority priority)
        {
            return TwoInletPipe<TMessageType>.CreatePrioritised(priority);
        }

        public ITwoInletRandomisedPipeBuilder<TMessageType> Randomised()
        {
            return new TwoInletRandomisedPipeBuilder<TMessageType>();
        }

        public ITwoInletPipe<TMessageType> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially)
        {
            return TwoInletPipe<TMessageType>.CreateAlternated(alternated);
        }

        public ITwoInletPipe<TMessageType> WithTieHandler(ITieBreaker tieBreaker)
        {
            return TwoInletPipe<TMessageType>.Create(tieBreaker);
        }
    }
}