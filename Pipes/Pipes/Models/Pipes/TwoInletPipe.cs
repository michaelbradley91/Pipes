using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoInletPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> LeftInlet { get; }
        ISimpleInlet<TMessage> RightInlet { get; }
        ISimpleOutlet<TMessage> Outlet { get; }
    }

    public abstract class TwoInletPipe<TMessage> : SimplePipe<TMessage>, ITwoInletPipe<TMessage>
    {
        public ISimpleInlet<TMessage> LeftInlet { get; }
        public ISimpleInlet<TMessage> RightInlet { get; }
        public ISimpleOutlet<TMessage> Outlet { get; }
        public override SharedResource SharedResource { get; }

        protected TwoInletPipe(ISimpleInlet<TMessage> leftInlet, ISimpleInlet<TMessage> rightInlet, ISimpleOutlet<TMessage> outlet)
        {
            LeftInlet = leftInlet;
            RightInlet = rightInlet;
            Outlet = outlet;

            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(LeftInlet.SharedResource, RightInlet.SharedResource, Outlet.SharedResource);
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(LeftInlet.SharedResource, pipeResource);
            resourceGroup.ConnectSharedResources(RightInlet.SharedResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, Outlet.SharedResource);
            
            resourceGroup.FreeSharedResources();

            SharedResource = pipeResource;
        }

        public override IReadOnlyCollection<IInlet> AllInlets => new[] {LeftInlet, RightInlet};
        public override IReadOnlyCollection<IOutlet> AllOutlets => new[] {Outlet};
    }
}