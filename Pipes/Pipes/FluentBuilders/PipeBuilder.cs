namespace Pipes.FluentBuilders
{
    public interface IPipeBuilder
    {
    }

    public class PipeBuilder : IPipeBuilder
    {
        internal PipeBuilder() {}

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