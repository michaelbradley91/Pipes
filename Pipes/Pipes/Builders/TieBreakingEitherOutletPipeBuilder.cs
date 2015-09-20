using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherOutletPipeBuilder<out TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        IEitherOutletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        private readonly TTieBreaker tieBreaker;

        public TieBreakingEitherOutletPipeBuilder(TTieBreaker tieBreaker)
        {
            this.tieBreaker = tieBreaker;
        }

        public IEitherOutletPipe<TTieBreaker, TMessage> Build()
        {
            return new EitherOutletPipe<TTieBreaker, TMessage>(tieBreaker);
        }
    }
}