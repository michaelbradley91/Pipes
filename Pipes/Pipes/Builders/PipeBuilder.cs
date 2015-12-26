namespace Pipes.Builders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder<TMessage> BasicPipe<TMessage>();
        ICapacityPipeBuilder<TMessage> CapacityPipe<TMessage>();
        IEitherInletPipeBuilder<TMessage> EitherInletPipe<TMessage>();
        IEitherOutletPipeBuilder<TMessage> EitherOutletPipe<TMessage>();
        ISplittingPipeBuilder<TMessage> SplittingPipe<TMessage>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        public static readonly IPipeBuilder New = new PipeBuilder();

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

        public ISplittingPipeBuilder<TMessage> SplittingPipe<TMessage>()
        {
            return new SplittingPipeBuilder<TMessage>();
        }
    }
}