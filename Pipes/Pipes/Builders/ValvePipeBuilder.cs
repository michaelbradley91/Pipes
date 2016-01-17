using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface IValvedPipeBuilder<TReceive, TSend>
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

        IValvedPipe<TReceive, TSend, IPrioritisingTwoWayTieBreaker> Build();

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker;

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTwoWayTieBreaker> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithPrioritisingTieBreaker(Priority priority = Priority.Left);

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithSendPriority();

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithReceivePriority();

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        ITieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTwoWayTieBreaker> WithRandomisingTieBreaker(double sendProbability = 0.5);
    }

    public class ValvedPipeBuilder<TReceive, TSend> : IValvedPipeBuilder<TReceive, TSend>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TReceive>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TSend>> Outlet { get; set; }

        public ValvedPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TReceive>(p);
            Outlet = p => new SimpleOutlet<TSend>(p);
        }

        public IValvedPipe<TReceive, TSend, IPrioritisingTwoWayTieBreaker> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker>(new PrioritisingTwoWayTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker>(tieBreaker));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTwoWayTieBreaker> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IAlternatingTwoWayTieBreaker>(new AlternatingTwoWayTieBreaker(alternated)));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker>(new PrioritisingTwoWayTieBreaker(priority)));
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithSendPriority()
        {
            return WithPrioritisingTieBreaker(Priority.Left);
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IPrioritisingTwoWayTieBreaker> WithReceivePriority()
        {
            return WithPrioritisingTieBreaker(Priority.Right);
        }

        public ITieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTwoWayTieBreaker> WithRandomisingTieBreaker(double sendProbability = 0.5)
        {
            if (sendProbability < 0 || sendProbability > 1) throw new ArgumentOutOfRangeException(nameof(sendProbability), "The send probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingValvedPipeBuilder<TReceive, TSend, IRandomisingTwoWayTieBreaker>(new RandomisingTwoWayTieBreaker(sendProbability)));
        }

        private ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingValvedPipeBuilder<TReceive, TSend, TTieBreaker> tieBreakingValvedPipeBuilder)
            where TTieBreaker : ITwoWayTieBreaker
        {
            tieBreakingValvedPipeBuilder.Inlet = Inlet;
            tieBreakingValvedPipeBuilder.Outlet = Outlet;
            return tieBreakingValvedPipeBuilder;
        }
    }
}