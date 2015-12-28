using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ICapacityPipeBuilder<TMessage>
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

        int Capacity { get; set; }

        ICapacityPipeBuilder<TMessage> WithCapacity(int capacity);

        ICapacityPipe<TMessage> Build();
    }

    public class CapacityPipeBuilder<TMessage> : ICapacityPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }
        public int Capacity { get; set; }

        public CapacityPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
            Outlet = p => new SimpleOutlet<TMessage>(p, SharedResourceHelpers.CreateSharedResource());
        }   

        public ICapacityPipe<TMessage> Build()
        {
            CapacityPipe<TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);
            var outlet = Outlet(lazyPipe);

            pipe[0] = new CapacityPipe<TMessage>(inlet, outlet, Capacity);

            return pipe[0];
        }

        public ICapacityPipeBuilder<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), "The pipeCapacity of a pipeCapacity pipe cannot be negative");
            Capacity = capacity;
            return this;
        }
    }
}