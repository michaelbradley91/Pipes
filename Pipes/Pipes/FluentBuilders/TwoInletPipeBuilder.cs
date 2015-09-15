using Pipes.Constants;
using Pipes.Models;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletPipeBuilder
    {
        ITwoInletPipe Prioritised(Priority priority);
        ITwoInletRandomisedPipeBuilder Randomised();
        ITwoInletPipe Alternated(Alternated alternating = Constants.Alternated.LeftHasPriorityInitially);
    }

    public class TwoInletPipeBuilder : ITwoInletPipeBuilder
    {
        internal TwoInletPipeBuilder()
        {
        }

        public ITwoInletPipe Prioritised(Priority priority)
        {
            return new TwoInletPipe();
        }

        public ITwoInletRandomisedPipeBuilder Randomised()
        {
            return new TwoInletRandomisedPipeBuilder();
        }

        public ITwoInletPipe Alternated(Alternated alternating = Constants.Alternated.LeftHasPriorityInitially)
        {
            return new TwoInletPipe();
        }
    }
}