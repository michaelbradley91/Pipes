using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IStraightPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
        ISimpleOutlet<TMessage> Outlet { get; }
    }

    public abstract class StraightPipe<TMessage> : SimplePipe<TMessage>, IStraightPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public ISimpleOutlet<TMessage> Outlet { get; }
        public override SharedResource SharedResource { get; }

        protected StraightPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> outlet)
        {
            Inlet = inlet;
            Outlet = outlet;

            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(Inlet.SharedResource, Outlet.SharedResource);
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();
            
            resourceGroup.ConnectSharedResources(inlet.SharedResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outlet.SharedResource);

            resourceGroup.FreeSharedResources();

            SharedResource = pipeResource;
        }

        public override IReadOnlyCollection<IInlet> AllInlets => new[] {Inlet};
        public override IReadOnlyCollection<IOutlet> AllOutlets => new[] {Outlet};
    }
}
