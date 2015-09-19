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

    public class SimpleSimplePipe<TMessage> : ISimplePipe<TMessage>, IPipe<TMessage>
    {
        private readonly Queue<TMessage> storedMessages;
        public readonly int Capacity;
        public readonly Inlet<TMessage> Inlet;
        public readonly Outlet<TMessage> Outlet;

        internal SimpleSimplePipe(int capacity)
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
                if (HasSpareCapacity())
                {
                    // TODO: need to pull other messages down
                    return message => storedMessages.Enqueue(message);
                }
                return null;
            }
            if (Outlet.ConnectedInlet == null)
            {
                if (Outlet.HasWaitingReceiver())
                {
                    // TODO: need to pull other messages down
                    return message => Outlet.UseWaitingReceiver(message);
                }
                if (HasSpareCapacity())
                {
                    // TODO: need to pull other messages down
                    return message => storedMessages.Enqueue(message);
                }
                return null;
            }
            var nextPipe = Outlet.ConnectedInlet.Pipe;
            var receiver = nextPipe.FindReceiver();
            if (receiver != null) return receiver;
            if (HasSpareCapacity())
            {
                // TODO: need to pull other messages down
                return message => storedMessages.Enqueue(message);
            }
            return null;
        }

        Func<TMessage> IPipe<TMessage>.FindSender()
        {
            if (storedMessages.Any())
            {
                // TODO: need to pull other messages down
                return () => storedMessages.Dequeue();
            }
            if (Capacity > 0) return null;
            if (Inlet.ConnectedOutlet == null)
            {
                if (Inlet.HasWaitingSender())
                {
                    // TODO: need to pull other messages down
                    return () => Inlet.UseWaitingSender();
                }
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