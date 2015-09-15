using Pipes.Constants;
using Pipes.Models.TieHandlers;

namespace Pipes.Models
{
    public interface ITwoInletPipe<TMessageType>
    {
        IInlet<TMessageType> LeftInlet { get; }
        IInlet<TMessageType> RightInlet { get; }
        IOutlet<TMessageType> Outlet { get; }
    }

    public class TwoInletPipe<TMessageType> : ITwoInletPipe<TMessageType>
    {
        public IInlet<TMessageType> LeftInlet { get; private set; }
        public IInlet<TMessageType> RightInlet { get; private set; }
        public IOutlet<TMessageType> Outlet { get; private set; }

        private readonly ITieHandler tieHandler;

        public ITieHandler GetCopyOfTieHandler()
        {
            return tieHandler.DeepCopy();
        }

        private TwoInletPipe(double leftProbability)
            : this(new RandomisingTieHandler(leftProbability))
        {
        }

        private TwoInletPipe(Priority priority)
            : this(new PrioritisingTieHandler(priority))
        {
        }

        private TwoInletPipe(Alternated alternated) 
            : this(new AlternatingTieHandler(alternated))
        {
        }

        private TwoInletPipe(ITieHandler tieHandler)
        {
            LeftInlet = new Inlet<TMessageType>();
            RightInlet = new Inlet<TMessageType>();
            Outlet = new Outlet<TMessageType>();
            this.tieHandler = tieHandler;
        }

        internal static TwoInletPipe<TMessageType> CreateRandomised(double leftProbability)
        {
            return new TwoInletPipe<TMessageType>(leftProbability);
        }

        internal static TwoInletPipe<TMessageType> CreatePrioritised(Priority priority)
        {
            return new TwoInletPipe<TMessageType>(priority);
        }

        internal static TwoInletPipe<TMessageType> CreateAlternated(Alternated alternated)
        {
            return new TwoInletPipe<TMessageType>(alternated);
        }

        internal static TwoInletPipe<TMessageType> Create(ITieHandler tieHandler)
        {
            return new TwoInletPipe<TMessageType>(tieHandler);
        }
    }
}