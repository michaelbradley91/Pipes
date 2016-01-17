using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the left inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TMessage>> LeftInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TMessage>> RightInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IEitherInletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> LeftInlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> RightInlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingEitherInletPipeBuilder(TTieBreaker tieBreaker)
        {
            LeftInlet = p => new SimpleInlet<TMessage>(p);
            RightInlet = p => new SimpleInlet<TMessage>(p);
            Outlet = p => new SimpleOutlet<TMessage>(p);
            TieBreaker = tieBreaker;
        }

        public IEitherInletPipe<TTieBreaker, TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var leftInlet = LeftInlet(promisedPipe);
            var rightInlet = RightInlet(promisedPipe);
            var outlet = Outlet(promisedPipe);

            return promisedPipe.Fulfill(new EitherInletPipe<TTieBreaker, TMessage>(leftInlet, rightInlet, outlet, TieBreaker));
        }
    }
}