using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IBasicPipe<TMessage> : IStraightPipe<TMessage>
    {
    }

    public class BasicPipe<TMessage> : StraightPipe<TMessage>, IBasicPipe<TMessage>
    {
        public BasicPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> outlet)
            : base(inlet, outlet)
        {
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return Outlet.FindReceiver();
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return Inlet.FindSender();
        }
    }
}