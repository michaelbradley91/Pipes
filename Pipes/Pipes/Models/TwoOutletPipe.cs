using System;
using Pipes.Constants;
using Pipes.Models.TieHandlers;

namespace Pipes.Models
{
    public interface ITwoOutletPipe<TMessageType>
    {
        IInlet<TMessageType> Inlet { get; }
        IOutlet<TMessageType> LeftOutlet { get; }
        IOutlet<TMessageType> RightOutlet { get; }
    }

    public class TwoOutletPipe<TMessageType> : ITwoOutletPipe<TMessageType>
    {
        public IInlet<TMessageType> Inlet { get; private set; }
        public IOutlet<TMessageType> LeftOutlet { get; private set; }
        public IOutlet<TMessageType> RightOutlet { get; private set; }

        private readonly ITieHandler tieHandler;

        public bool HasTieHandler()
        {
            return tieHandler != null;
        }

        public ITieHandler GetCopyOfTieHandler()
        {
            if (tieHandler == null) throw new InvalidOperationException("The two outlet pipe does not have a tie handler when it is set to duplicate messages received in its inlet");
            return tieHandler.DeepCopy();
        }

        private TwoOutletPipe(double leftProbability)
            : this(new RandomisingTieHandler(leftProbability))
        {
        }

        private TwoOutletPipe(Priority priority)
            : this(new PrioritisingTieHandler(priority))
        {
        }

        private TwoOutletPipe(Alternated alternated) 
            : this(new AlternatingTieHandler(alternated))
        {
        }

        private TwoOutletPipe(ITieHandler tieHandler)
        {
            Inlet = new Inlet<TMessageType>();
            LeftOutlet = new Outlet<TMessageType>();
            RightOutlet = new Outlet<TMessageType>();
            this.tieHandler = tieHandler;
        }

        internal static TwoOutletPipe<TMessageType> CreateRandomised(double leftProbability)
        {
            return new TwoOutletPipe<TMessageType>(leftProbability);
        }

        internal static TwoOutletPipe<TMessageType> CreatePrioritised(Priority priority)
        {
            return new TwoOutletPipe<TMessageType>(priority);
        }

        internal static TwoOutletPipe<TMessageType> CreateAlternated(Alternated alternated)
        {
            return new TwoOutletPipe<TMessageType>(alternated);
        }

        internal static TwoOutletPipe<TMessageType> CreateDuplicator()
        {
            return new TwoOutletPipe<TMessageType>(null);
        }

        internal static TwoOutletPipe<TMessageType> Create(ITieHandler tieHandler)
        {
            return new TwoOutletPipe<TMessageType>(tieHandler);
        }
    }
}