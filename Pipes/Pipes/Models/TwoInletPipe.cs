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
            : this()
        {
            tieHandler = new RandomisingTieHandler(leftProbability);
        }

        private TwoInletPipe(Priority priority)
            : this()
        {
            tieHandler = new PrioritisingTieHandler(priority);
        }

        private TwoInletPipe(Alternated alternated) 
            : this()
        {
            tieHandler = new AlternatingTieHandler(alternated);
        }

        private TwoInletPipe()
        {
            LeftInlet = new Inlet<TMessageType>();
            RightInlet = new Inlet<TMessageType>();
            Outlet = new Outlet<TMessageType>();
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
    }
}