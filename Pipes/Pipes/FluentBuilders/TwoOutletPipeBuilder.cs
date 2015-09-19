﻿using Pipes.Constants;
using Pipes.Models;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.FluentBuilders
{
    public interface ITwoOutletPipeBuilder<TMessage>
    {
        ITwoOutletPipe<TMessage> Prioritised(Priority priority);
        ITwoOutletRandomisedPipeBuilder<TMessage> Randomised();
        ITwoOutletPipe<TMessage> Alternated(Alternated alternated = Constants.Alternated.LeftHasPriorityInitially);
        ITwoOutletPipe<TMessage> Duplicating();
    }

    public class TwoOutletPipeBuilder<TMessageType> : ITwoOutletPipeBuilder<TMessageType>
    {
        internal TwoOutletPipeBuilder() { }

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

        public ITwoOutletPipe<TMessageType> WithTieHandler(ITieBreaker tieBreaker)
        {
            return TwoOutletPipe<TMessageType>.Create(tieBreaker);
        }
    }
}