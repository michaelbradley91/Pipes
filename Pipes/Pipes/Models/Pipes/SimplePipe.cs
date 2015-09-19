using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ISimplePipe<TMessage>
    {
        int Capacity { get; }
        Inlet<TMessage> Inlet { get; }
        Outlet<TMessage> Outlet { get; }
    }

    public class SimplePipe<TMessage> : ISimplePipe<TMessage>, IPipe<TMessage>
    {
        private readonly Queue<TMessage> storedMessages;
        public int Capacity { get; private set; }
        public Inlet<TMessage> Inlet { get; private set; }
        public Outlet<TMessage> Outlet { get; private set; }

        internal SimplePipe(int capacity)
        {
            Capacity = capacity;
            storedMessages = new Queue<TMessage>();

            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var inletResource = resourceGroup.CreateAndAcquireSharedResource();
            var outletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(inletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outletResource);

            Inlet = new Inlet<TMessage>(this, inletResource);
            Outlet = new Outlet<TMessage>(this, outletResource);

            resourceGroup.FreeSharedResources();
        }

        IReadOnlyCollection<Inlet<TMessage>> IPipe<TMessage>.Inlets
        {
            get { return new[] {Inlet}; } 
        }

        IReadOnlyCollection<Outlet<TMessage>> IPipe<TMessage>.Outlets
        {
            get { return new[] {Outlet}; }
        }

        Action<TMessage> IPipe<TMessage>.FindReceiver()
        {
            if (storedMessages.Any())
            {
                if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);

                return null;
            }

            if (Outlet.ConnectedInlet == null)
            {
                if (Outlet.HasWaitingReceiver()) return message => Outlet.UseWaitingReceiver(message);

                if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);

                return null;
            }

            var nextPipe = Outlet.ConnectedInlet.Pipe;
            var receiver = nextPipe.FindReceiver();
            if (receiver != null) return receiver;

            if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);

            return null;
        }

        Func<TMessage> IPipe<TMessage>.FindSender()
        {
            if (storedMessages.Any())
            {
                return () =>
                {
                    var message = storedMessages.Dequeue();

                    if (storedMessages.Count == Capacity - 1)
                    {
                        if (Inlet.ConnectedOutlet == null)
                        {
                            if (Inlet.HasWaitingSender()) storedMessages.Enqueue(Inlet.UseWaitingSender());
                        }
                        else
                        {
                            var sender = Inlet.ConnectedOutlet.Pipe.FindSender();
                            if (sender != null) storedMessages.Enqueue(sender());
                        }
                    }

                    return message;
                };
            }

            if (Capacity > 0) return null;

            if (Inlet.ConnectedOutlet == null)
            {
                if (Inlet.HasWaitingSender()) return () => Inlet.UseWaitingSender();
                return null;
            }

            var previousPipe = Inlet.ConnectedOutlet.Pipe;
            return previousPipe.FindSender();
        }

        private bool HasSpareCapacity()
        {
            return storedMessages.Count < Capacity;
        }
    }
}