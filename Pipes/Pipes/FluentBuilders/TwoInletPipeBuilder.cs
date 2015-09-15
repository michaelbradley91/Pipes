using Pipes.Constants;
using Pipes.Models;

namespace Pipes.FluentBuilders
{
    public interface ITwoInletPipeBuilder
    {
        ITwoInletPipe Prioritised(Priority priority);
        ITwoInletRandomisedPipeBuilder Randomised();
        ITwoInletPipe Alternating(Alternating alternating = Constants.Alternating.LeftHasPriorityInitially);
    }

    public class TwoInletPipeBuilder : ITwoInletPipeBuilder
    {
        internal TwoInletPipeBuilder() { }

        public ITwoInletPipe Prioritised(Priority priority)
        {
            return new TwoInletPipe();
        }

        public ITwoInletRandomisedPipeBuilder Randomised()
        {
            return new TwoInletRandomisedPipeBuilder();
        }

        public ITwoInletPipe Alternating(Alternating alternating = Constants.Alternating.LeftHasPriorityInitially)
        {
            return new TwoInletPipe();
        }
    }
}