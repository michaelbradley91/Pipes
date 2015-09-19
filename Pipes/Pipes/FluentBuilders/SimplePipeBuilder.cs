using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface ISimplePipeBuilder<TMessageType>
    {
        ISimplePipe<TMessageType> WithNoCapacity();
        ISimplePipe<TMessageType> WithCapacity(int capacity);
    }

    public class SimplePipeBuilder<TMessage> : ISimplePipeBuilder<TMessage>
    {
        internal SimplePipeBuilder()
        {
        }

        public ISimplePipe<TMessage> WithNoCapacity()
        {
            return new SimplePipe<TMessage>(0);
        }

        public ISimplePipe<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new SimplePipe<TMessage>(capacity);
        }
    }
}