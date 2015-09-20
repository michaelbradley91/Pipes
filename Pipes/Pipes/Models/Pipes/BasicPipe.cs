using System;

namespace Pipes.Models.Pipes
{
    public interface IBasicPipe<TMessage> : ISimplePipe<TMessage>
    {
    }

    public class BasicPipe<TMessage> : SimplePipe<TMessage>, IBasicPipe<TMessage>
    {
        public override Action<TMessage> FindReceiver()
        {
            return Outlet.FindReceiver();
        }

        public override Func<TMessage> FindSender()
        {
            return Inlet.FindSender();
        }
    }
}