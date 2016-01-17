using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the left outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> LeftOutlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> RightOutlet { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IEitherOutletPipe<TTieBreaker, TMessage> Build();
    }

    public class TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> : ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> LeftOutlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> RightOutlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingEitherOutletPipeBuilder(TTieBreaker tieBreaker)
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
            LeftOutlet = p => new SimpleOutlet<TMessage>(p);
            RightOutlet = p => new SimpleOutlet<TMessage>(p);
            TieBreaker = tieBreaker;
        }

        public IEitherOutletPipe<TTieBreaker, TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = Inlet(promisedPipe);
            var leftOutlet = LeftOutlet(promisedPipe);
            var rightOutlet = RightOutlet(promisedPipe);

            return promisedPipe.Fulfill(new EitherOutletPipe<TTieBreaker, TMessage>(inlet, leftOutlet, rightOutlet, TieBreaker));
        }
    }
}