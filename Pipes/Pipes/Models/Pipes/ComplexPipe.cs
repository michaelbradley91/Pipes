using System;
using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public abstract class ComplexPipe<TSourceMessage,TTargetMessage> : Pipe
    {
        protected ComplexPipe(IReadOnlyCollection<IInlet<TSourceMessage>> inlets, IReadOnlyCollection<IOutlet<TTargetMessage>> outlets)
            : base(inlets, outlets)
        {
        }

        protected override Action<T> FindReceiverFor<T>(IInlet<T> inletSendingMessage)
        {
            var inlet = (IInlet<TSourceMessage>) inletSendingMessage;
            var receiver = FindReceiver(inlet);
            if (receiver == null) return null;
            return m => receiver((TSourceMessage) (object) m);
        }

        protected override Func<T> FindSenderFor<T>(IOutlet<T> outletReceivingMessage)
        {
            var outlet = (IOutlet<TTargetMessage>) outletReceivingMessage;
            var sender = FindSender(outlet);
            if (sender == null) return null;
            return () => (T) (object) sender();
        }

        protected abstract Action<TSourceMessage> FindReceiver(IInlet<TSourceMessage> inletSendingMessage);
        protected abstract Func<TTargetMessage> FindSender(IOutlet<TTargetMessage> outletReceivingMessage);
    }
}