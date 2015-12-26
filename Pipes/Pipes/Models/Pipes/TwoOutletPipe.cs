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

    public abstract class TwoOutletPipe<TMessage> : Pipe<TMessage>, ITwoOutletPipe<TMessage>
    {
        public IInlet<TMessage> Inlet { get; }
        public IOutlet<TMessage> LeftOutlet { get; }
        public IOutlet<TMessage> RightOutlet { get; }
        public override SharedResource SharedResource { get; }

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

            SharedResource = pipeResource;
        }

        public override IReadOnlyCollection<IInlet<TMessage>> Inlets => new[] {Inlet};
        public override IReadOnlyCollection<IOutlet<TMessage>> Outlets => new[] {LeftOutlet, RightOutlet};
    }
}