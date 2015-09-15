namespace Pipes.FluentBuilders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder NewPipe();
        ITwoInletPipeBuilder NewTwoInletPipe();
        ITwoOutletPipeBuilder NewTwoOutletPipe();
    }

    public class PipeBuilder : IPipeBuilder
    {
        internal PipeBuilder() {}

        public static readonly IPipeBuilder Create = new PipeBuilder();

        public IBasicPipeBuilder NewPipe()
        {
            return new BasicPipeBuilder();
        }

        public ITwoInletPipeBuilder NewTwoInletPipe()
        {
            return new TwoInletPipeBuilder();
        }

        public ITwoOutletPipeBuilder NewTwoOutletPipe()
        {
            return new TwoOutletPipeBuilder();
        }
    }
}