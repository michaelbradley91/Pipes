namespace Pipes.FluentBuilders
{
    public interface IPipeBuilder
    {
        ISimplePipeBuilder<TMessage> NewPipe<TMessage>();
        ITwoInletPipeBuilder<TMessage> NewTwoInletPipe<TMessage>();
        ITwoOutletPipeBuilder<TMessage> NewTwoOutletPipe<TMessage>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        internal PipeBuilder()
        {
        }

        public static readonly IPipeBuilder Create = new PipeBuilder();

        public ISimplePipeBuilder<TMessage> NewPipe<TMessage>()
        {
            return new SimplePipeBuilder<TMessage>();
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