using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
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
            EitherInletPipe<TTieBreaker, TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);

            var leftInlet = new Inlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var rightInlet = new Inlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var outlet = new Outlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());

            pipe[0] = new EitherInletPipe<TTieBreaker, TMessage>(leftInlet, rightInlet, outlet, tieBreaker);

            return pipe[0];
        }
    }
}