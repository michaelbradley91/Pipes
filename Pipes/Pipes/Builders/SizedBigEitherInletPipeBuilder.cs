using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ISizedBigEitherInletPipeBuilder<TMessage>
    {
        /// <summary>
        /// The number of inlets that will be created in the big either inlet pipe.
        /// </summary>
        int Size { get; set; }

        ITieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;

        IBigEitherInletPipe<TMessage, IMinPrioritisingTieBreaker> Build();
    }

    public class SizedBigEitherInletPipeBuilder<TMessage> : ISizedBigEitherInletPipeBuilder<TMessage>
    {
        public int Size { get; set; }

        public SizedBigEitherInletPipeBuilder(int size)
        {
            Size = size;
        }

        public ITieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker>(Size, tieBreaker);
        }

        public IBigEitherInletPipe<TMessage, IMinPrioritisingTieBreaker> Build()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            return new TieBreakingSizedBigEitherInletPipeBuilder<TMessage, IMinPrioritisingTieBreaker>(Size, tieBreaker).Build();
        }
    }
}