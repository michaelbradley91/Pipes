using System;
using Pipes.Constants;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface IEitherOutletPipeBuilder<TMessage>
    {
        IEitherOutletPipe<IPrioritisingTieBreaker, TMessage> Build();
        ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;
        ITieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated);
        ITieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority);
        ITieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability);
    }

    public class EitherOutletPipeBuilder<TMessage> : IEitherOutletPipeBuilder<TMessage>
    {
        public IEitherOutletPipe<IPrioritisingTieBreaker, TMessage> Build()
        {
            return new EitherOutletPipe<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(Priority.Left));
        }

        public ITieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingEitherOutletPipeBuilder<TTieBreaker, TMessage>(tieBreaker);
        }

        public ITieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage> WithAlternatingTieBreaker(Alternated alternated)
        {
            return new TieBreakingEitherOutletPipeBuilder<IAlternatingTieBreaker, TMessage>(new AlternatingTieBreaker(alternated));
        }

        public ITieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage> WithPrioritisingTieBreaker(Priority priority)
        {
            return new TieBreakingEitherOutletPipeBuilder<IPrioritisingTieBreaker, TMessage>(new PrioritisingTieBreaker(priority));
        }

        public ITieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage> WithRandomisingTieBreaker(double leftProbability)
        {
            if (leftProbability < 0 || leftProbability > 1) throw new ArgumentOutOfRangeException("leftProbability", "The left probability must be between 0 and 1 (inclusive)");
            return new TieBreakingEitherOutletPipeBuilder<IRandomisingTieBreaker, TMessage>(new RandomisingTieBreaker(leftProbability));
        }
    }
}