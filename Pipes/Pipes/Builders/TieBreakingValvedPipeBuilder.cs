using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> where TTieBreaker : ITwoWayTieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        TTieBreaker TieBreaker { get; set; }

        IValvedPipe<TReceive, TSend, TTieBreaker> Build();
    }

    public class TieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> : ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> where TTieBreaker : ITwoWayTieBreaker
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingValvedPipeBuilder(TTieBreaker tieBreaker)
        {
            Inlet = p => new SimpleInlet<TReceive>(p);
            Outlet = p => new SimpleOutlet<TSend>(p);
            TieBreaker = tieBreaker;
        }

        public IValvedPipe<TReceive, TSend, TTieBreaker> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = Inlet(promisedPipe);
            var outlet = Outlet(promisedPipe);

            return promisedPipe.Fulfill(new ValvedPipe<TReceive, TSend, TTieBreaker>(inlet, outlet, TieBreaker));
        }
    }
}