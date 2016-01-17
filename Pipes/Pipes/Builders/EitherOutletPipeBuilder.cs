using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface IEitherOutletPipeBuilder<TMessage>
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

        IEitherOutletPipe<IPrioritisingTwoWayTieBreaker, TMessage> Build();
        ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker;
        ITieBreakingEitherOutletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingEitherOutletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingEitherOutletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class EitherOutletPipeBuilder<TMessage> : IEitherOutletPipeBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> LeftOutlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> RightOutlet { get; set; }

        public EitherOutletPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
            LeftOutlet = p => new SimpleOutlet<TMessage>(p);
            RightOutlet = p => new SimpleOutlet<TMessage>(p);
        }

        public IEitherOutletPipe<IPrioritisingTwoWayTieBreaker, TMessage> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage>(new PrioritisingTwoWayTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITwoWayTieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage>(tieBreaker));
        }

        public ITieBreakingEitherOutletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IAlternatingTwoWayTieBreaker, TMessage>(new AlternatingTwoWayTieBreaker(alternated)));
        }

        public ITieBreakingEitherOutletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IPrioritisingTwoWayTieBreaker, TMessage>(new PrioritisingTwoWayTieBreaker(priority)));
        }

        public ITieBreakingEitherOutletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException(nameof(leftProbability), "The left probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingEitherOutletPipeBuilder<IRandomisingTwoWayTieBreaker, TMessage>(new RandomisingTwoWayTieBreaker(leftProbability)));
        }

        private ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> tieBreakingEitherOutletPipeBuilder)
            where TTieBreaker : ITwoWayTieBreaker
        {
            tieBreakingEitherOutletPipeBuilder.Inlet = Inlet;
            tieBreakingEitherOutletPipeBuilder.LeftOutlet = LeftOutlet;
            tieBreakingEitherOutletPipeBuilder.RightOutlet = RightOutlet;
            return tieBreakingEitherOutletPipeBuilder;
        }
    }
}