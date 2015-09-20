namespace Pipes.Builders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder<TMessage> Build<TMessage>();
        IBasicPipeBuilder<TMessage> BasicPipe<TMessage>();
        ICapacityPipeBuilder<TMessage> CapacityPipe<TMessage>();
        IEitherInletPipeBuilder<TMessage> EitherInletPipe<TMessage>();
        IEitherOutletPipeBuilder<TMessage> EitherOutletPipe<TMessage>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        public static readonly IPipeBuilder New = new PipeBuilder();

        public IBasicPipeBuilder<TMessage> Build<TMessage>()
        {
            return new BasicPipeBuilder<TMessage>();
        }

        public IBasicPipeBuilder<TMessage> BasicPipe<TMessage>()
        {
            return new BasicPipeBuilder<TMessage>();
        }

        public ICapacityPipeBuilder<TMessage> CapacityPipe<TMessage>()
        {
            return new CapacityPipeBuilder<TMessage>();
        }

        public IEitherInletPipeBuilder<TMessage> EitherInletPipe<TMessage>()
        {
            return new EitherInletPipeBuilder<TMessage>();
        }

        public IEitherOutletPipeBuilder<TMessage> EitherOutletPipe<TMessage>()
        {
            return new EitherOutletPipeBuilder<TMessage>();
        }
    }
}