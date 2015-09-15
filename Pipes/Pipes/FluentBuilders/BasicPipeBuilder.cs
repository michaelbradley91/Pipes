using Pipes.Models;
using System;

namespace Pipes.FluentBuilders
{
    public interface IBasicPipeBuilder<TMessageType>
    {
        IPipe<TMessageType> WithNoCapacity();
        IPipe<TMessageType> WithCapacity(int capacity);
    }

    public class BasicPipeBuilder<TMessageType> : IBasicPipeBuilder<TMessageType>
    {
        internal BasicPipeBuilder()
        {
        }

        public IPipe<TMessageType> WithNoCapacity()
        {
            return new Pipe<TMessageType>(0);
        }

        public IPipe<TMessageType> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new Pipe<TMessageType>(capacity);
        }
    }
}