using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the left inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> LeftInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> RightInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> Outlet { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IEitherInletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        public Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> LeftInlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IInlet<TMessage>> RightInlet { get; set; }
        public Func<Lazy<IPipe<TMessage>>, IOutlet<TMessage>> Outlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingEitherInletPipeBuilder(TTieBreaker tieBreaker)
        {
            LeftInlet = p => new Inlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            RightInlet = p => new Inlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            Outlet = p => new Outlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            TieBreaker = tieBreaker;
        }

        public IEitherInletPipe<TTieBreaker, TMessage> Build()
        {
            EitherInletPipe<TTieBreaker, TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);

            var leftInlet = LeftInlet(lazyPipe);
            var rightInlet = RightInlet(lazyPipe);
            var outlet = Outlet(lazyPipe);

            pipe[0] = new EitherInletPipe<TTieBreaker, TMessage>(leftInlet, rightInlet, outlet, TieBreaker);

            return pipe[0];
        }
    }
}