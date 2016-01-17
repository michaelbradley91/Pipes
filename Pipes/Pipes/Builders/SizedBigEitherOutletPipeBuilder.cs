using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ISizedBigEitherOutletPipeBuilder<TMessage>
    {
        /// <summary>
        /// The number of outlets that will be created in the big either outlet pipe.
        /// </summary>
        int Size { get; set; }

        ITieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;

        IBigEitherOutletPipe<TMessage, IMinPrioritisingTieBreaker> Build();
    }

    public class SizedBigEitherOutletPipeBuilder<TMessage> : ISizedBigEitherOutletPipeBuilder<TMessage>
    {
        public int Size { get; set; }

        public SizedBigEitherOutletPipeBuilder(int size)
        {
            Size = size;
        }

        public ITieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker>(Size, tieBreaker);
        }

        public IBigEitherOutletPipe<TMessage, IMinPrioritisingTieBreaker> Build()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            return new TieBreakingSizedBigEitherOutletPipeBuilder<TMessage, IMinPrioritisingTieBreaker>(Size, tieBreaker).Build();
        }
    }
}