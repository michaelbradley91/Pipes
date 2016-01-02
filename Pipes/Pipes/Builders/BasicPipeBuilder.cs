using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Builders
{
    public interface IBasicPipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        IBasicPipe<TMessage> Build();
    }

    public class BasicPipeBuilder<TMessage> : IBasicPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        public BasicPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p, SharedResource.Create());
            Outlet = p => new SimpleOutlet<TMessage>(p, SharedResource.Create());
        }

        public IBasicPipe<TMessage> Build()
        {
            BasicPipe<TMessage>[] pipe = {null};
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);
            
            var inlet = Inlet(lazyPipe);
            var outlet = Outlet(lazyPipe);

            pipe[0] = new BasicPipe<TMessage>(inlet, outlet);

            return pipe[0];
        }
    }
}