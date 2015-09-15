using System;
using Pipes.Models;

namespace Pipes.FluentBuilders
{
    public interface IBasicPipeBuilder
    {
        IPipe WithNoCapacity();
        IPipe WithCapacity(int capacity);
    }

    public class BasicPipeBuilder : IBasicPipeBuilder
    {
        internal BasicPipeBuilder() {}

        public IPipe WithNoCapacity()
        {
            return new Pipe(0);
        }

        public IPipe WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            return new Pipe(capacity);
        }
    }
}