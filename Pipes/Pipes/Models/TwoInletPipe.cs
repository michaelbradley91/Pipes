using Pipes.Constants;
using Pipes.Models.TieHandlers;

namespace Pipes.Models
{
    public interface ITwoInletPipe
    {
        IInlet LeftInlet { get; }
        IInlet RightInlet { get; }
        IOutlet Outlet { get; }
    }

    public class TwoInletPipe : ITwoInletPipe
    {
        public IInlet LeftInlet { get; private set; }
        public IInlet RightInlet { get; private set; }
        public IOutlet Outlet { get; private set; }

        private readonly ITieHandler tieHandler; 

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
            LeftInlet = new Inlet();
            RightInlet = new Inlet();
            Outlet = new Outlet();
        }

        internal static TwoInletPipe CreateRandomised(double leftProbability)
        {
            return new TwoInletPipe(leftProbability);
        }

        internal static TwoInletPipe CreatePrioritised(Priority priority)
        {
            return new TwoInletPipe(priority);
        }

        internal static TwoInletPipe CreateAlternated(Alternated alternated)
        {
            return new TwoInletPipe(alternated);
        }
    }
}