using System;
using Pipes.Constants;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IEitherInletPipeBuilder<TMessage>
    {
        IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build();
        ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated);
        ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority);
        ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability);
    }

    public class EitherInletPipeBuilder<TMessage> : IEitherInletPipeBuilder<TMessage>
    {
        public IEitherInletPipe<IPrioritisingTieBreaker, TMessage> Build()
        {
            return new EitherInletPipe<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(Priority.Left));
        }

        public ITieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingEitherInletPipeBuilder<TTieBreaker, TMessage>(tieBreaker);
        }

        public ITieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated)
        {
            return new TieBreakingEitherInletPipeBuilder<IAlternatingTieBreaker, TMessage>(new AlternatingTieBreaker(alternated));
        }

        public ITieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority)
        {
            return new TieBreakingEitherInletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(priority));
        }

        public ITieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException("leftProbability", "The left probability must be between 0 and 1 (inclusive)");
            return new TieBreakingEitherInletPipeBuilder<IRandomisingTieBreaker, TMessage>(new RandomisingTieBreaker(leftProbability));
        }
    }
}