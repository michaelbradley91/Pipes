using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IEitherInletPipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the left inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TMessage>> LeftInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TMessage>> RightInlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build();
        ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class EitherInletPipeBuilder<TMessage> : IEitherInletPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> LeftInlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> RightInlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        public EitherInletPipeBuilder()
        {
            LeftInlet = p => new SimpleInlet<TMessage>(p);
            RightInlet = p => new SimpleInlet<TMessage>(p);
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }

        public IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build()
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(Priority.Left))).Build();
        }

        public ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage>(tieBreaker));
        }

        public ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage>(new AlternatingTieBreaker(alternated)));
        }

        public ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(priority)));
        }

        public ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException(nameof(leftProbability), "The left probability must be between 0 and 1 (inclusive)");
            return CopyInletsAndOutletsTo(new TieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage>(new RandomisingTieBreaker(leftProbability)));
        }

        private ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> CopyInletsAndOutletsTo<TTieBreaker>(ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> tieBreakingEitherInletPipeBuilder)
            where TTieBreaker : ITieBreaker
        {
            tieBreakingEitherInletPipeBuilder.LeftInlet = LeftInlet;
            tieBreakingEitherInletPipeBuilder.RightInlet = RightInlet;
            tieBreakingEitherInletPipeBuilder.Outlet = Outlet;
            return tieBreakingEitherInletPipeBuilder;
        }
    }
}