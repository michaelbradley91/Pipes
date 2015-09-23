using System;
using Pipes.Constants;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IEitherInletPipeBuilder<TMessage>
    {
        IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build();
        ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially);
        ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left);
        ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5);
    }

    public class EitherInletPipeBuilder<TMessage> : IEitherInletPipeBuilder<TMessage>
    {
        public IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build()
        {
            return new TieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(Priority.Left)).Build();
        }

        public ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage>(tieBreaker);
        }

        public ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated = Alternated.LeftHasPriorityInitially)
        {
            return new TieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage>(new AlternatingTieBreaker(alternated));
        }

        public ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority = Priority.Left)
        {
            return new TieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(priority));
        }

        public ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability = 0.5)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException("leftProbability", "The left probability must be between 0 and 1 (inclusive)");
            return new TieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage>(new RandomisingTieBreaker(leftProbability));
        }
    }
}