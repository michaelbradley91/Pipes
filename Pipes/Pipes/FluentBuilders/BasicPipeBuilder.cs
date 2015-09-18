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

    public class BasicPipeBuilder<TMessageType> : IBasicPipeBuilder<TMessageType>
    {
        internal BasicPipeBuilder()
        {
        }

        public ISimplePipe<TMessageType> WithNoCapacity()
        {
            return new SimpleSimplePipe<TMessageType>(0);
        }

        public ISimplePipe<TMessageType> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new SimpleSimplePipe<TMessageType>(capacity);
        }
    }
}