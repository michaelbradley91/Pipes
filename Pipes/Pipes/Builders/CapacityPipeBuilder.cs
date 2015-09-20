using System;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ICapacityPipeBuilder<TMessage>
    {
        ICapacityPipe<TMessage> Build();
        void WithCapacity(int capacity);
    }

    public class CapacityPipeBuilder<TMessage> : ICapacityPipeBuilder<TMessage>
    {
        private int pipeCapacity;

        public ICapacityPipe<TMessage> Build()
        {
            return new CapacityPipe<TMessage>(pipeCapacity);
        }

        public void WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", "The pipeCapacity of a pipeCapacity pipe cannot be negative");
            pipeCapacity = capacity;
        }
    }
}