using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Builders
{
    public interface ISizedBigValvedPipeBuilder<TReceive, TSend>
    {
        int NumberOfInlets { get; set; }
        int NumberOfOutlets { get; set; }

        ITieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker;

        IBigValvedPipe<TReceive, TSend, IMinPrioritisingTieBreaker> Build();
    }

    public class SizedBigValvedPipeBuilder<TReceive, TSend> : ISizedBigValvedPipeBuilder<TReceive, TSend>
    {
        public int NumberOfInlets { get; set; }
        public int NumberOfOutlets { get; set; }

        public SizedBigValvedPipeBuilder(int numberOfInlets, int numberOfOutlets)
        {
            NumberOfInlets = numberOfInlets;
            NumberOfOutlets = numberOfOutlets;
        }
       
        public ITieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker> WithTieBreaker<TTieBreaker>(TTieBreaker tieBreaker) where TTieBreaker : ITieBreaker
        {
            return new TieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker>(NumberOfInlets, NumberOfOutlets, tieBreaker);
        }

        public IBigValvedPipe<TReceive, TSend, IMinPrioritisingTieBreaker> Build()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            return new TieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, IMinPrioritisingTieBreaker>(NumberOfInlets, NumberOfOutlets, tieBreaker).Build();
        }
    }
}