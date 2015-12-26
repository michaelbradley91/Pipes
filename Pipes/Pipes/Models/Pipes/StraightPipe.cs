using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IStraightPipe<TMessage> : IPipe<TMessage>
    {
        IInlet<TMessage> Inlet { get; }
        IOutlet<TMessage> Outlet { get; }
    }

    public abstract class StraightPipe<TMessage> : Pipe<TMessage>, IStraightPipe<TMessage>
    {
        public IInlet<TMessage> Inlet { get; }
        public IOutlet<TMessage> Outlet { get; }

        protected StraightPipe(IInlet<TMessage> inlet, IOutlet<TMessage> outlet)
        {
            Inlet = inlet;
            Outlet = outlet;

            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(Inlet.SharedResource, Outlet.SharedResource);
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(inlet.SharedResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outlet.SharedResource);

            resourceGroup.FreeSharedResources();
        }

        public override IReadOnlyCollection<IInlet<TMessage>> Inlets => new[] {Inlet};

        public override IReadOnlyCollection<IOutlet<TMessage>> Outlets => new[] {Outlet};
    }
}
