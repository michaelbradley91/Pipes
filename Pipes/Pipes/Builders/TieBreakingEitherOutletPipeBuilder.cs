using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
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
            EitherOutletPipe<TTieBreaker, TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);

            var inlet = new Inlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var leftOutlet = new Outlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var rightOutlet = new Outlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());

            pipe[0] = new EitherOutletPipe<TTieBreaker, TMessage>(inlet, leftOutlet, rightOutlet, tieBreaker);

            return pipe[0];
        }
    }
}