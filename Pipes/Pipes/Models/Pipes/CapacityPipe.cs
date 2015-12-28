using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ICapacityPipe<TMessage> : IStraightPipe<TMessage>
    {
        int Capacity { get; }
        IReadOnlyList<TMessage> StoredMessages { get; }
    }

    public class CapacityPipe<TMessage> : StraightPipe<TMessage>, ICapacityPipe<TMessage>
    {
        public int Capacity { get; }
        private readonly Queue<TMessage> storedMessages;
        public IReadOnlyList<TMessage> StoredMessages => storedMessages.ToArray();

        public CapacityPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> outlet, int capacity)
            : base(inlet, outlet)
        {
            Capacity = capacity;
            storedMessages = new Queue<TMessage>();
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            if (storedMessages.Any())
            {
                if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);
            }
            else
            {
                var receiver = Outlet.FindReceiver();
                if (receiver != null) return receiver;

                if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);
            }
            return null;
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            if (storedMessages.Any())
            {
                return () =>
                {
                    var message = storedMessages.Dequeue();

                    if (storedMessages.Count == Capacity - 1)
                    {
                        var sender = Inlet.FindSender();
                        if (sender != null) storedMessages.Enqueue(sender());
                    }

                    return message;
                };
            }

            return Capacity > 0 ? null : Inlet.FindSender();
        }

        private bool HasSpareCapacity()
        {
            return storedMessages.Count < Capacity;
        }
    }
}