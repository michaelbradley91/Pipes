using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ISinkPipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        ISinkPipe<TMessage> Build();
    }
    
    public class SinkPipeBuilder<TMessage> : ISinkPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        public SinkPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
        }

        public ISinkPipe<TMessage> Build()
        {
            SinkPipe<TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);

            pipe[0] = new SinkPipe<TMessage>(inlet);

            return pipe[0];
        }
    }
}