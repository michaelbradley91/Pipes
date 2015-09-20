using System;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ICapacityPipeBuilder<TMessage>
    {
        void WithCapacity(int capacity);
        ICapacityPipe<TMessage> Build();
    }

    public class CapacityPipeBuilder<TMessage> : ICapacityPipeBuilder<TMessage>
    {
        private int pipeCapacity;

        public void WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", "The pipeCapacity of a pipeCapacity pipe cannot be negative");
            pipeCapacity = capacity;
        }

        public ICapacityPipe<TMessage> Build()
        {
            return new CapacityPipe<TMessage>(pipeCapacity);
        }
    }
}