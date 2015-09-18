using Pipes.Models;
using System;
using Pipes.Models.Pipes;

namespace Pipes.FluentBuilders
{
    public interface IBasicPipeBuilder<TMessageType>
    {
        ISimplePipe<TMessageType> WithNoCapacity();
        ISimplePipe<TMessageType> WithCapacity(int capacity);
    }

    public class BasicPipeBuilder<TMessage> : IBasicPipeBuilder<TMessage>
    {
        internal BasicPipeBuilder()
        {
        }

        public ISimplePipe<TMessage> WithNoCapacity()
        {
            return new SimpleSimplePipe<TMessage>(0);
        }

        public ISimplePipe<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new SimpleSimplePipe<TMessage>(capacity);
        }
    }
}