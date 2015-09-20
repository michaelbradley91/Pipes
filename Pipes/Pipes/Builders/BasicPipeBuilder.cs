using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface IBasicPipeBuilder<TMessage>
    {
        IBasicPipe<TMessage> Build();
    }

    public class BasicPipeBuilder<TMessage> : IBasicPipeBuilder<TMessage>
    {
        public IBasicPipe<TMessage> Build()
        {
            return new BasicPipe<TMessage>();
        }
    }
}