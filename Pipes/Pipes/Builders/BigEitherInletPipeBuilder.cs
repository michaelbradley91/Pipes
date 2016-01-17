using System;

namespace Pipes.Builders
{
    public interface IBigEitherInletPipeBuilder<TMessage>
    {
        ISizedBigEitherInletPipeBuilder<TMessage> WithSize(int size);
    }

    public class BigEitherInletPipeBuilder<TMessage> : IBigEitherInletPipeBuilder<TMessage>
    {
        public ISizedBigEitherInletPipeBuilder<TMessage> WithSize(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), "A big either inlet pipe must have at least one inlet");

            return new SizedBigEitherInletPipeBuilder<TMessage>(size);
        }
    }
}