using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
        ISimpleOutlet<TMessage> LeftOutlet { get; }
        ISimpleOutlet<TMessage> RightOutlet { get; }
    }

    public abstract class TwoOutletPipe<TMessage> : SimplePipe<TMessage>, ITwoOutletPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public ISimpleOutlet<TMessage> LeftOutlet { get; }
        public ISimpleOutlet<TMessage> RightOutlet { get; }
        public override SharedResource SharedResource { get; }

        protected TwoOutletPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> leftOutlet, ISimpleOutlet<TMessage> rightOutlet)
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

        public override IReadOnlyCollection<IInlet> AllInlets => new[] {Inlet};
        public override IReadOnlyCollection<IOutlet> AllOutlets => new[] {LeftOutlet, RightOutlet};
    }
}