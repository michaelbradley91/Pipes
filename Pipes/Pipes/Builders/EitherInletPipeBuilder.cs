using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface IEitherInletPipeBuilder<TMessage>
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

        IEitherInletPipe<IPrioritisingTwoWayTieBreaker, TMessage> Build();
        ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker;
        ITieBreakingEitherInletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingEitherInletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingEitherInletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class EitherInletPipeBuilder<TMessage> : IEitherInletPipeBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> LeftInlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> RightInlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        public EitherInletPipeBuilder()
        {
            LeftInlet = p => new SimpleInlet<TMessage>(p);
            RightInlet = p => new SimpleInlet<TMessage>(p);
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }

        public IEitherInletPipe<IPrioritisingTwoWayTieBreaker, TMessage> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage>(new PrioritisingTwoWayTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage>(tieBreaker));
        }

        public ITieBreakingEitherInletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage>(new AlternatingTwoWayTieBreaker(alternated)));
        }

        public ITieBreakingEitherInletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage>(new PrioritisingTwoWayTieBreaker(priority)));
        }

        public ITieBreakingEitherInletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException(nameof(leftProbability), "The left probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage>(new RandomisingTwoWayTieBreaker(leftProbability)));
        }

        private ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> tieBreakingEitherInletPipeBuilder)
            where TTieBreaker : ITwoWayTieBreaker
        {
            tieBreakingEitherInletPipeBuilder.LeftInlet = LeftInlet;
            tieBreakingEitherInletPipeBuilder.RightInlet = RightInlet;
            tieBreakingEitherInletPipeBuilder.Outlet = Outlet;
            return tieBreakingEitherInletPipeBuilder;
        }
    }
}