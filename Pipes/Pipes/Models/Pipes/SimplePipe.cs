using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public abstract class SimplePipe<TMessage> : Pipe
    {
        protected override Action<T> FindReceiverFor<T>(IInlet<T> inletSendingMessage)
        {
            var inlet = (IInlet<TMessage>) inletSendingMessage;
            var receiver = FindReceiver(inlet);
            if (receiver == null) return null;
            return m => receiver((TMessage) (object) m);
        }

        protected override Func<T> FindSenderFor<T>(IOutlet<T> outletReceivingMessage)
        {
            var outlet = (IOutlet<TMessage>) outletReceivingMessage;
            var sender = FindSender(outlet);
            if (sender == null) return null;
            return () => (T) (object) sender();
        }

        protected abstract Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage);
        protected abstract Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage);
    }
}