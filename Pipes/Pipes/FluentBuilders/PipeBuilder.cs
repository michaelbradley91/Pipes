namespace Pipes.FluentBuilders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder<TMessageType> NewPipe<TMessageType>();
        ITwoInletPipeBuilder<TMessageType> NewTwoInletPipe<TMessageType>();
        ITwoOutletPipeBuilder<TMessageType> NewTwoOutletPipe<TMessageType>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        internal PipeBuilder()
        {
        }

        public static readonly IPipeBuilder Create = new PipeBuilder();

        public IBasicPipeBuilder<TMessageType> NewPipe<TMessageType>()
        {
            return new BasicPipeBuilder<TMessageType>();
        }

        public ITwoInletPipeBuilder<TMessageType> NewTwoInletPipe<TMessageType>()
        {
            return new TwoInletPipeBuilder<TMessageType>();
        }

        public ITwoOutletPipeBuilder<TMessageType> NewTwoOutletPipe<TMessageType>()
        {
            return new TwoOutletPipeBuilder<TMessageType>();
        }
    }
}