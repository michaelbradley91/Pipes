using System;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        int NumberOfInlets { get; set; }
        int NumberOfOutlets { get; set; }
        TTieBreaker TieBreaker { get; set; }

        IBigValvedPipe<TReceive, TSend, TTieBreaker> Build();
    }

    public class TieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker> : ITieBreakingSizedBigValvedPipeBuilder<TReceive, TSend, TTieBreaker>
        where TTieBreaker : ITieBreaker
    {
        public int NumberOfInlets { get; set; }
        public int NumberOfOutlets { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingSizedBigValvedPipeBuilder(int numberOfInlets, int numberOfOutlets, TTieBreaker tieBreaker)
        {
            NumberOfInlets = numberOfInlets;
            NumberOfOutlets = numberOfOutlets;
            TieBreaker = tieBreaker;
        }
        
        public IBigValvedPipe<TReceive, TSend, TTieBreaker> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlets = Enumerable.Repeat<Func<SimpleInlet<TReceive>>>(() => new SimpleInlet<TReceive>(promisedPipe), NumberOfInlets).Select(f => f()).ToList();
            var outlets = Enumerable.Repeat<Func<SimpleOutlet<TSend>>>(() => new SimpleOutlet<TSend>(promisedPipe), NumberOfOutlets).Select(f => f()).ToList();

            return promisedPipe.Fulfill(new BigValvedPipe<TReceive, TSend, TTieBreaker>(inlets, outlets, TieBreaker));
        }
    }
}