using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ISimplePipe<TMessageType>
    {
        int Capacity { get; }
        Inlet<TMessageType> Inlet { get; }
        Outlet<TMessageType> Outlet { get; }
    }

    public class SimpleSimplePipe<TMessageType> : ISimplePipe<TMessageType>, IPipe<TMessageType>
    {
        public int Capacity { get; private set; }
        public Inlet<TMessageType> Inlet { get; private set; }
        public Outlet<TMessageType> Outlet { get; private set; }

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

            Inlet = new Inlet<TMessageType>(this, inletResource);
            Outlet = new Outlet<TMessageType>(this, outletResource);

            resourceGroup.FreeSharedResources();
        }

        IReadOnlyCollection<Inlet<TMessageType>> IPipe<TMessageType>.Inlets
        {
            get { return new[] {Inlet}; } 
        }

        IReadOnlyCollection<Outlet<TMessageType>> IPipe<TMessageType>.Outlets
        {
            get { return new[] {Outlet}; }
        }
    }
}