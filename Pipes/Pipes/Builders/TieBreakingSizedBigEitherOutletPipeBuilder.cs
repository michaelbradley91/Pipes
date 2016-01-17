using System;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        /// <summary>
        /// The number of outlets that will be created in the big either outlet pipe.
        /// </summary>
        int Size { get; set; }

        TTieBreaker TieBreaker { get; set; }

        IBigEitherOutletPipe<TMessage, TTieBreaker> Build();
    }

    public class TieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker> : ITieBreakingSizedBigEitherOutletPipeBuilder<TMessage, TTieBreaker> 
        where TTieBreaker : ITieBreaker
    {
        public int Size { get; set; }
        public TTieBreaker TieBreaker { get; set; }

        public TieBreakingSizedBigEitherOutletPipeBuilder(int size, TTieBreaker tieBreaker)
        {
            Size = size;
            TieBreaker = tieBreaker;
        }

        public IBigEitherOutletPipe<TMessage, TTieBreaker> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = new SimpleInlet<TMessage>(promisedPipe);
            var outlets = Enumerable.Repeat<Func<SimpleOutlet<TMessage>>>(() => new SimpleOutlet<TMessage>(promisedPipe), Size).Select(f => f()).ToList();

            return promisedPipe.Fulfill(new BigEitherOutletPipe<TMessage, TTieBreaker>(inlet, outlets, TieBreaker));
        }
    }
}