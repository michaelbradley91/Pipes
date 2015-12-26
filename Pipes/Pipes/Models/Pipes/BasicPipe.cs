using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IBasicPipe<TMessage> : IStraightPipe<TMessage>
    {
    }

    public class BasicPipe<TMessage> : StraightPipe<TMessage>, IBasicPipe<TMessage>
    {
        public BasicPipe(IInlet<TMessage> inlet, IOutlet<TMessage> outlet)
            : base(inlet, outlet)
        {
        }

        public override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return Outlet.FindReceiver();
        }

        public override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return Inlet.FindSender();
        }
    }
}