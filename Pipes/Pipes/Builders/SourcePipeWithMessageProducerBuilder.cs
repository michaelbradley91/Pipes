using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ISourcePipeWithMessageProducerBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        /// <summary>
        /// This function may be run by pipes any number of times while resolving where a message
        /// is sent. It should also not rely on the acquisition of any other pipes.
        /// 
        /// Generally, this should be a simple stateless function - for the same input it should return the same output.
        /// Try other functions at your own risk
        /// </summary>
        Func<TMessage> MessageProducer { get; set; }

        ISourcePipe<TMessage> Build();
    }

    public class SourcePipeWithMessageProducerBuilder<TMessage> : ISourcePipeWithMessageProducerBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }
        public Func<TMessage> MessageProducer { get; set; }

        public SourcePipeWithMessageProducerBuilder(Func<TMessage> messageProducer)
        {
            MessageProducer = messageProducer;
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }

        public ISourcePipe<TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var outlet = Outlet(promisedPipe);

            return promisedPipe.Fulfill(new SourcePipe<TMessage>(outlet, MessageProducer));
        }
    }
}