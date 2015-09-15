using Pipes.FluentBuilders;

namespace Pipes
{
    public static class Pipes
    {
        public static IPipeBuilder Create
        {
            get
            {
                return new PipeBuilder();
            }
        }
    }
}