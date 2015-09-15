using Pipes.Constants;
using Pipes.Models.TieHandlers;

namespace Pipes.Models
{
    public interface ITwoOutletPipe
    {
        IInlet Inlet { get; }
        IOutlet LeftOutlet { get; }
        IOutlet RightOutlet { get; }
    }

    public class TwoOutletPipe : ITwoOutletPipe
    {
        public IInlet Inlet { get; private set; }
        public IOutlet LeftOutlet { get; private set; }
        public IOutlet RightOutlet { get; private set; }

        private readonly ITieHandler tieHandler; 

        private TwoOutletPipe(double leftProbability)
            : this()
        {
            tieHandler = new RandomisingTieHandler(leftProbability);
        }

        private TwoOutletPipe(Priority priority)
            : this()
        {
            tieHandler = new PrioritisingTieHandler(priority);
        }

        private TwoOutletPipe(Alternated alternated) 
            : this()
        {
            tieHandler = new AlternatingTieHandler(alternated);
        }

        internal TwoOutletPipe()
        {
            Inlet = new Inlet();
            LeftOutlet = new Outlet();
            RightOutlet = new Outlet();
        }

        internal static TwoOutletPipe CreateRandomised(double leftProbability)
        {
            return new TwoOutletPipe(leftProbability);
        }

        internal static TwoOutletPipe CreatePrioritised(Priority priority)
        {
            return new TwoOutletPipe(priority);
        }

        internal static TwoOutletPipe CreateAlternated(Alternated alternated)
        {
            return new TwoOutletPipe(alternated);
        }

        internal static TwoOutletPipe CreateDuplicator()
        {
            return new TwoOutletPipe();
        }
    }
}