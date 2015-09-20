using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface ISimplePipeBuilder<TMessageType>
    {
        ICapacityPipe<TMessageType> WithNoCapacity();
        ICapacityPipe<TMessageType> WithCapacity(int capacity);
    }

    public class SimplePipeBuilder<TMessage> : ISimplePipeBuilder<TMessage>
    {
        internal SimplePipeBuilder()
        {
        }

        public ICapacityPipe<TMessage> WithNoCapacity()
        {
            return new CapacityPipe<TMessage>(0);
        }

        public ICapacityPipe<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new CapacityPipe<TMessage>(capacity);
        }
    }
}