using System;

namespace Pipes.Builders
{
    public interface IBigEitherOutletPipeBuilder<TMessage>
    {
        ISizedBigEitherOutletPipeBuilder<TMessage> WithSize(int size);
    }

    public class BigEitherOutletPipeBuilder<TMessage> : IBigEitherOutletPipeBuilder<TMessage>
    {
        public ISizedBigEitherOutletPipeBuilder<TMessage> WithSize(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), "A big either outlet pipe must have at least one outlet");

            return new SizedBigEitherOutletPipeBuilder<TMessage>(size);
        }
    }
}