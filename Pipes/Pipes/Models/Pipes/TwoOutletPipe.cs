using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessage> : IPipe<TMessage>
    {
        Inlet<TMessage> Inlet { get; }
        Outlet<TMessage> LeftOutlet { get; }
        Outlet<TMessage> RightOutlet { get; }
    }

    public abstract class TwoOutletPipe<TMessage> : ITwoOutletPipe<TMessage>
    {
        public Inlet<TMessage> Inlet { get; private set; }
        public Outlet<TMessage> LeftOutlet { get; private set; }
        public Outlet<TMessage> RightOutlet { get; private set; }

        protected TwoOutletPipe()
        {
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var inletResource = resourceGroup.CreateAndAcquireSharedResource();
            var leftOutletResource = resourceGroup.CreateAndAcquireSharedResource();
            var rightOutletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(inletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, leftOutletResource);
            resourceGroup.ConnectSharedResources(pipeResource, rightOutletResource);

            Inlet = new Inlet<TMessage>(this, inletResource);
            LeftOutlet = new Outlet<TMessage>(this, leftOutletResource);
            RightOutlet = new Outlet<TMessage>(this, rightOutletResource);

            resourceGroup.FreeSharedResources();
        }
        IReadOnlyCollection<Inlet<TMessage>> IPipe<TMessage>.Inlets
        {
            get { return new[] {Inlet}; }
        }

        IReadOnlyCollection<Outlet<TMessage>> IPipe<TMessage>.Outlets
        {
            get { return new[] {LeftOutlet, RightOutlet}; }
        }

        public abstract Action<TMessage> FindReceiver();
        public abstract Func<TMessage> FindSender();
    }
}