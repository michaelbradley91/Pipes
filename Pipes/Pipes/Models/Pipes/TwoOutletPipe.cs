using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessage> : IPipe<TMessage>
    {
        IInlet<TMessage> Inlet { get; }
        IOutlet<TMessage> LeftOutlet { get; }
        IOutlet<TMessage> RightOutlet { get; }
    }

    public abstract class TwoOutletPipe<TMessage> : ITwoOutletPipe<TMessage>
    {
        public IInlet<TMessage> Inlet { get; private set; }
        public IOutlet<TMessage> LeftOutlet { get; private set; }
        public IOutlet<TMessage> RightOutlet { get; private set; }

        protected TwoOutletPipe(IInlet<TMessage> inlet, IOutlet<TMessage> leftOutlet, IOutlet<TMessage> rightOutlet)
        {
            Inlet = inlet;
            LeftOutlet = leftOutlet;
            RightOutlet = rightOutlet;

            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(Inlet.SharedResource, LeftOutlet.SharedResource, RightOutlet.SharedResource);
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(Inlet.SharedResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, LeftOutlet.SharedResource);
            resourceGroup.ConnectSharedResources(pipeResource, RightOutlet.SharedResource);

            resourceGroup.FreeSharedResources();
        }

        IReadOnlyCollection<IInlet<TMessage>> IPipe<TMessage>.Inlets
        {
            get { return new[] {Inlet}; }
        }

        IReadOnlyCollection<IOutlet<TMessage>> IPipe<TMessage>.Outlets
        {
            get { return new[] {LeftOutlet, RightOutlet}; }
        }

        public abstract Action<TMessage> FindReceiver();
        public abstract Func<TMessage> FindSender();
    }
}