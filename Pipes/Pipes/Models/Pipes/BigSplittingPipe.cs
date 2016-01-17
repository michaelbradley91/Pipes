using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IBigSplittingPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
        IReadOnlyCollection<ISimpleOutlet<TMessage>> Outlets { get; }
    }

    public class BigSplittingPipe<TMessage> : SimplePipe<TMessage>, IBigSplittingPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public IReadOnlyCollection<ISimpleOutlet<TMessage>> Outlets { get; }

        public BigSplittingPipe(ISimpleInlet<TMessage> inlet, IReadOnlyCollection<ISimpleOutlet<TMessage>> outlets)
            : base(new[] {inlet}, outlets)
        {
            Inlet = inlet;
            Outlets = outlets;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            var receivers = Outlets.Select(o => o.FindReceiver()).Where(r => r != null).ToList();
            if (receivers.Count != Outlets.Count) return null;
            
            return message =>
            {
                foreach (var receiver in receivers)
                {
                    receiver(message);
                }
            };
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            var sender = Inlet.FindSender();
            if (sender == null) return null;

            var otherReceivers = Outlets.Where(o => o != outletReceivingMessage).Select(o => o.FindReceiver()).Where(r => r != null).ToList();

            if (otherReceivers.Count != Outlets.Count - 1) return null;

            return () =>
            {
                var message = sender();

                foreach (var otherReceiver in otherReceivers)
                {
                    otherReceiver(message);
                }

                return message;
            };
        }
    }
}