using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        TTieBreaker TieBreaker { get; set; }

        IValvedPipe<TReceive, TSend, TTieBreaker> Build();
    }

    public class TieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> : ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingValvedPipeBuilder(TTieBreaker tieBreaker)
        {
            Inlet = p => new SimpleInlet<TReceive>(p, SharedResourceHelpers.CreateSharedResource());
            Outlet = p => new SimpleOutlet<TSend>(p, SharedResourceHelpers.CreateSharedResource());
            TieBreaker = tieBreaker;
        }

        public IValvedPipe<TReceive, TSend, TTieBreaker> Build()
        {
            ValvedPipe<TReceive, TSend, TTieBreaker>[] pipe = {null};
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);
            var outlet = Outlet(lazyPipe);

            pipe[0] = new ValvedPipe<TReceive, TSend, TTieBreaker>(inlet, outlet, TieBreaker);

            return pipe[0];
        }
    }
}