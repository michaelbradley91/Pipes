using Pipes.Constants;
using Pipes.Models;

namespace Pipes.FluentBuilders
{
    public interface ITwoOutletPipeBuilder
    {
        ITwoOutletPipe Prioritised(Priority priority);
        ITwoInletRandomisedPipeBuilder Randomised();
        ITwoOutletPipe Alternated(Alternated alternating = Constants.Alternated.LeftHasPriorityInitially);
        ITwoOutletPipe Duplicating();
    }

    public class TwoOutletPipeBuilder : ITwoOutletPipeBuilder
    {
        internal TwoOutletPipeBuilder()
        {
        }

        public ITwoOutletPipe Prioritised(Priority priority)
        {
            return new TwoOutletPipe();
        }

        public ITwoInletRandomisedPipeBuilder Randomised()
        {
            return new TwoInletRandomisedPipeBuilder();
        }

        public ITwoOutletPipe Alternated(Alternated alternating = Constants.Alternated.LeftHasPriorityInitially)
        {
            return new TwoOutletPipe();
        }

        public ITwoOutletPipe Duplicating()
        {
            return new TwoOutletPipe();
        }
    }
}