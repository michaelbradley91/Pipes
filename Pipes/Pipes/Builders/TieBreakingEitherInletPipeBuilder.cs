using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherInletPipeBuilder<out TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        IEitherInletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        private readonly TTieBreaker tieBreaker;

        public TieBreakingEitherInletPipeBuilder(TTieBreaker tieBreaker)
        {
            this.tieBreaker = tieBreaker;
        }

        public IEitherInletPipe<TTieBreaker, TMessage> Build()
        {
            return new EitherInletPipe<TTieBreaker, TMessage>(tieBreaker);
        }
    }
}