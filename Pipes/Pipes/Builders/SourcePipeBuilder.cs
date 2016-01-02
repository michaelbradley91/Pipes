using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ISourcePipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        /// <summary>
        /// The function passed in may be run by pipes any number of times while resolving where a message
        /// is sent. It should also not rely on the acquisition of any other pipes.
        /// 
        /// Generally, this should be a simple stateless function - for the same input it should return the same output.
        /// Try other functions at your own risk
        /// </summary>
        ISourcePipeWithMessageProducerBuilder<TMessage> WithMessageProducer(Func<TMessage> messageProducer);
    }

    public class SourcePipeBuilder<TMessage> : ISourcePipeBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        public SourcePipeBuilder()
        {
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }

        public ISourcePipeWithMessageProducerBuilder<TMessage> WithMessageProducer(Func<TMessage> messageProducer)
        {
            var sourcePipeWithMessageProducerBuilder = new SourcePipeWithMessageProducerBuilder<TMessage>(messageProducer)
            {
                Outlet = Outlet
            };
            return sourcePipeWithMessageProducerBuilder;
        }
    }
}