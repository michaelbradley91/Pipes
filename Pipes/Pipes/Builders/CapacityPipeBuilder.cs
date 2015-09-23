using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Builders
{
    public interface ICapacityPipeBuilder<TMessage>
    {
        ICapacityPipe<TMessage> Build();
        ICapacityPipeBuilder<TMessage> WithCapacity(int capacity);
    }

    public class CapacityPipeBuilder<TMessage> : ICapacityPipeBuilder<TMessage>
    {
        private int pipeCapacity;

        public ICapacityPipe<TMessage> Build()
        {
            CapacityPipe<TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);

            var inlet = new Inlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var outlet = new Outlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());

            pipe[0] = new CapacityPipe<TMessage>(inlet, outlet, pipeCapacity);

            return pipe[0];
        }

        public ICapacityPipeBuilder<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", "The pipeCapacity of a pipeCapacity pipe cannot be negative");
            pipeCapacity = capacity;
            return this;
        }
    }
}