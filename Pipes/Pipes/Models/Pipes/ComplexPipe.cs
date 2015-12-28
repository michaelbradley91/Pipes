using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IComplexPipe<TSourceMessage, TTargetMessage> : IPipe
    {
        ISimpleInlet<TSourceMessage> Inlet { get; }
        ISimpleOutlet<TTargetMessage> Outlet { get; }
    }

    public abstract class ComplexPipe<TSourceMessage,TTargetMessage> : Pipe, IComplexPipe<TSourceMessage, TTargetMessage>
    {
        public ISimpleInlet<TSourceMessage> Inlet { get; }
        public ISimpleOutlet<TTargetMessage> Outlet { get; }
        public override SharedResource SharedResource { get; }

        protected ComplexPipe(ISimpleInlet<TSourceMessage> inlet, ISimpleOutlet<TTargetMessage> outlet)
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

        protected override Action<T> FindReceiverFor<T>(IInlet<T> inletSendingMessage)
        {
            var inlet = (IInlet<TSourceMessage>) inletSendingMessage;
            var receiver = FindReceiver(inlet);
            if (receiver == null) return null;
            return m => receiver((TSourceMessage) (object) m);
        }

        protected override Func<T> FindSenderFor<T>(IOutlet<T> outletReceivingMessage)
        {
            var outlet = (IOutlet<TTargetMessage>) outletReceivingMessage;
            var sender = FindSender(outlet);
            if (sender == null) return null;
            return () => (T) (object) sender();
        }

        protected abstract Action<TSourceMessage> FindReceiver(IInlet<TSourceMessage> inletSendingMessage);
        protected abstract Func<TTargetMessage> FindSender(IOutlet<TTargetMessage> outletReceivingMessage);

        public override IReadOnlyCollection<IInlet> AllInlets => new[] {Inlet};
        public override IReadOnlyCollection<IOutlet> AllOutlets => new[] {Outlet};
    }
}