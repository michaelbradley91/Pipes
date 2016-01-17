using System;

namespace Pipes.Builders
{
    public interface IBigSplittingPipeBuilder<TMessage>
    {
        ISizedBigSplittingPipeBuilder<TMessage> WithSize(int size);
    }

    public class BigSplittingPipeBuilder<TMessage> : IBigSplittingPipeBuilder<TMessage>
    {
        public ISizedBigSplittingPipeBuilder<TMessage> WithSize(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), "A big splitting pipe must have at least one outlet");

            return new SizedBigSplittingPipeBuilder<TMessage>(size);
        }
    }
}