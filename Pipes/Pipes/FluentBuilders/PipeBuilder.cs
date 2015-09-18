namespace Pipes.FluentBuilders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder<TMessage> NewPipe<TMessage>();
        ITwoInletPipeBuilder<TMessage> NewTwoInletPipe<TMessage>();
        ITwoOutletPipeBuilder<TMessage> NewTwoOutletPipe<TMessage>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        internal PipeBuilder()
        {
        }

        public static readonly IPipeBuilder Create = new PipeBuilder();

        public IBasicPipeBuilder<TMessage> NewPipe<TMessage>()
        {
            return new BasicPipeBuilder<TMessage>();
        }

        public ITwoInletPipeBuilder<TMessage> NewTwoInletPipe<TMessage>()
        {
            return new TwoInletPipeBuilder<TMessage>();
        }

        public ITwoOutletPipeBuilder<TMessage> NewTwoOutletPipe<TMessage>()
        {
            return new TwoOutletPipeBuilder<TMessage>();
        }
    }
}