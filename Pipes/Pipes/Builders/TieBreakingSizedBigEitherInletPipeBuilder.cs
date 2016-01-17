using System;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        /// <summary>
        /// The number of inlets that will be created in the big either inlet pipe.
        /// </summary>
        int Size { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IBigEitherInletPipe<TMessage, TTieBreaker> Build();
    }

    public class TieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker> : ITieBreakingSizedBigEitherInletPipeBuilder<TMessage, TTieBreaker>
        where TTieBreaker : ITieBreaker
    {
        public int Size { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingSizedBigEitherInletPipeBuilder(int size, TTieBreaker tieBreaker)
        {
            Size = size;
            TieBreaker = tieBreaker;
        }

        public IBigEitherInletPipe<TMessage, TTieBreaker> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlets = Enumerable.Repeat<Func<SimpleInlet<TMessage>>>(() => new SimpleInlet<TMessage>(promisedPipe), Size).Select(f => f()).ToList();
            var outlet = new SimpleOutlet<TMessage>(promisedPipe);

            return promisedPipe.Fulfill(new BigEitherInletPipe<TMessage, TTieBreaker>(inlets, outlet, TieBreaker));
        }
    }
}