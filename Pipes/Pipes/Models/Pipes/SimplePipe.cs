using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ISimplePipe<TMessage> : IPipe<TMessage>
    {
        Inlet<TMessage> Inlet { get; }
        Outlet<TMessage> Outlet { get; }
    }

    public abstract class SimplePipe<TMessage> : ISimplePipe<TMessage>
    {
        public Inlet<TMessage> Inlet { get; private set; }
        public Outlet<TMessage> Outlet { get; private set; }

        protected SimplePipe()
        {
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var inletResource = resourceGroup.CreateAndAcquireSharedResource();
            var outletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(inletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outletResource);

            Inlet = new Inlet<TMessage>(this, inletResource);
            Outlet = new Outlet<TMessage>(this, outletResource);
        }

        public IReadOnlyCollection<Inlet<TMessage>> Inlets
        {
            get { return new[] {Inlet}; }
        }

        public IReadOnlyCollection<Outlet<TMessage>> Outlets
        {
            get { return new[] {Outlet}; }
        }

        public abstract Action<TMessage> FindReceiver();
        public abstract Func<TMessage> FindSender();
    }
}
