using System.Collections.Generic;
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
        public int Capacity { get; private set; }
        public Inlet<TMessage> Inlet { get; private set; }
        public Outlet<TMessage> Outlet { get; private set; }

        internal SimpleSimplePipe(int capacity)
        {
            Capacity = capacity;

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

        void IPipe<TMessage>.TryToSend(WaitingSender<TMessage> sender)
        {
            throw new System.NotImplementedException();
        }

        void IPipe<TMessage>.TryToReceive(WaitingReceiver<TMessage> receiver)
        {
            throw new System.NotImplementedException();
        }
    }
}