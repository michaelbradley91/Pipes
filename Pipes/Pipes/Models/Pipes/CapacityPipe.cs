﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipes.Models.Pipes
{
    public interface ICapacityPipe<TMessage> : ISimplePipe<TMessage>
    {
        int Capacity { get; }
    }

    public class CapacityPipe<TMessage> : SimplePipe<TMessage>, ICapacityPipe<TMessage>
    {
        private readonly Queue<TMessage> storedMessages;
        public int Capacity { get; private set; }

        internal CapacityPipe(int capacity)
        {
            Capacity = capacity;
            storedMessages = new Queue<TMessage>();
        }

        public override Action<TMessage> FindReceiver()
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

        public override Func<TMessage> FindSender()
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