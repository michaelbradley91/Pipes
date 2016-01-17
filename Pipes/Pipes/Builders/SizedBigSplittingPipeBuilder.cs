using System;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ISizedBigSplittingPipeBuilder<TMessage>
    {
        /// <summary>
        /// The number of outlets that will be created in the big splitting pipe.
        /// </summary>
        int Size { get; set; }

        IBigSplittingPipe<TMessage> Build();
    }

    public class SizedBigSplittingPipeBuilder<TMessage> : ISizedBigSplittingPipeBuilder<TMessage>
    {
        public int Size { get; set; }

        public SizedBigSplittingPipeBuilder(int size)
        {
            Size = size;
        }

        public IBigSplittingPipe<TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = new SimpleInlet<TMessage>(promisedPipe);
            var outlets = Enumerable.Repeat<Func<SimpleOutlet<TMessage>>>(() => new SimpleOutlet<TMessage>(promisedPipe), Size).Select(f => f()).ToList();

            return promisedPipe.Fulfill(new BigSplittingPipe<TMessage>(inlet, outlets));
        }
    }
}