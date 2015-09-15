using Pipes.Constants;
using Pipes.Models;
using Pipes.Models.TieHandlers;

namespace Pipes.FluentBuilders
{
    public interface ITwoOutletPipeBuilder<TMessageType>
    {
        ITwoOutletPipe<TMessageType> Prioritised(Priority priority);
        ITwoOutletRandomisedPipeBuilder<TMessageType> Randomised();
        ITwoOutletPipe<TMessageType> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially);
        ITwoOutletPipe<TMessageType> Duplicating();
    }

    public class TwoOutletPipeBuilder<TMessageType> : ITwoOutletPipeBuilder<TMessageType>
    {
        internal TwoOutletPipeBuilder()
        {
        }

        public ITwoOutletPipe<TMessageType> Prioritised(Priority priority)
        {
            return TwoOutletPipe<TMessageType>.CreatePrioritised(priority);
        }

        public ITwoOutletRandomisedPipeBuilder<TMessageType> Randomised()
        {
            return new TwoOutletRandomisedPipeBuilder<TMessageType>();
        }

        public ITwoOutletPipe<TMessageType> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially)
        {
            return TwoOutletPipe<TMessageType>.CreateAlternated(alternated);
        }

        public ITwoOutletPipe<TMessageType> Duplicating()
        {
            return TwoOutletPipe<TMessageType>.CreateDuplicator();
        }

        public ITwoOutletPipe<TMessageType> WithTieHandler(ITieHandler tieHandler)
        {
            return TwoOutletPipe<TMessageType>.Create(tieHandler);
        }
    }
}