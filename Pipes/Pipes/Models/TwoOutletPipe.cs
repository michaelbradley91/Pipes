using System;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

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

        private readonly ITieBreaker tieBreaker;

        public bool HasTieBreaker()
        {
            return tieBreaker != null;
        }

        public ITieBreaker GetCopyOfTieBreaker()
        {
            if (tieBreaker == null) throw new InvalidOperationException("The two outlet pipe does not have a tie handler when it is set to duplicate messages received in its inlet");
            return tieBreaker.DeepCopy();
        }

        private TwoOutletPipe(double leftProbability)
            : this(new RandomisingTieBreaker(leftProbability))
        {
        }

        private TwoOutletPipe(Priority priority)
            : this(new PrioritisingTieBreaker(priority))
        {
        }

        private TwoOutletPipe(Alternated alternated) 
            : this(new AlternatingTieBreaker(alternated))
        {
        }

        private TwoOutletPipe(ITieBreaker tieBreaker)
        {
            Inlet = new Inlet<TMessageType>();
            LeftOutlet = new Outlet<TMessageType>();
            RightOutlet = new Outlet<TMessageType>();
            this.tieBreaker = tieBreaker;
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

        internal static TwoOutletPipe<TMessageType> Create(ITieBreaker tieBreaker)
        {
            return new TwoOutletPipe<TMessageType>(tieBreaker);
        }
    }
}