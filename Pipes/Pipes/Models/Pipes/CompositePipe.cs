using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.Utilities;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public abstract class CompositePipe : Pipe
    {
        protected override IReadOnlyCollection<IInlet> PipeInlets => ConnectableInlets.Concat(adapterInlets).ToList();
        protected override IReadOnlyCollection<IOutlet> PipeOutlets => ConnectableOutlets.Concat(adapterOutlets).ToList();

        private readonly IList<IInlet> adapterInlets;
        private readonly IList<IOutlet> adapterOutlets;  
        private readonly IBiLookup<IInlet, IOutlet> inletOutletBiLookup; 

        protected CompositePipe(IReadOnlyCollection<IInlet> connectableInlets, IReadOnlyCollection<IOutlet> connectableOutlets) : base(connectableInlets, connectableOutlets)
        {
            adapterInlets = new List<IInlet>();
            adapterOutlets = new List<IOutlet>();
            inletOutletBiLookup = new BiLookup<IInlet, IOutlet>();
        }

        /// <summary>
        /// A convenience method for creating an adapter for an outlet pair.
        /// This will automatically connect the adapter to the internal outlet and pass events between the adapter and external outlet.
        /// 
        /// The external outlet should be the outlet the world can interact with, while the internal outlet is the corresponding outlet of
        /// our internal pipe system - the implementation of this pipe.
        /// </summary>
        protected IAdapterInlet<TMessage> CreateAndConnectAdapter<TMessage>(IOutlet<TMessage> internalOutlet, IOutlet<TMessage> externalOutlet)
        {
            var adapterInlet =  new AdapterInlet<TMessage>(new Lazy<IPipe>(() => this));
            ConnectSharedResource(adapterInlet.SharedResource);

            adapterInlets.Add(adapterInlet);
            inletOutletBiLookup.Add(adapterInlet, externalOutlet);

            // Do not check if the graph forms a tree - it probably doesn't as all adapters are connected
            // to this pipe. However, messages should never be passed in a cycle or violate the "tree-ness" nevertheless.
            adapterInlet.ConnectTo(internalOutlet, false);
            return adapterInlet;
        }

        /// <summary>
        /// A convenience method for creating an adapter for an inlet pair.
        /// This will automatically connect the adapter to the internal inlet and pass events between the adapter and external inlet.
        /// 
        /// The external inlet should be the inlet the world can interact with, while the internal inlet is the corresponding inlet of
        /// our internal pipe system - the implementation of this pipe.
        /// </summary>
        protected IAdapterOutlet<TMessage> CreateAndConnectAdapter<TMessage>(IInlet<TMessage> internalInlet, IInlet<TMessage> externalInlet)
        {
            var adapterOutlet = new AdapterOutlet<TMessage>(new Lazy<IPipe>(() => this));
            ConnectSharedResource(adapterOutlet.SharedResource);

            adapterOutlets.Add(adapterOutlet);
            inletOutletBiLookup.Add(externalInlet, adapterOutlet);

            // Do not check if the graph forms a tree - it probably doesn't as all adapters are connected
            // to this pipe. However, messages should never be passed in a cycle or violate the "tree-ness" nevertheless.
            adapterOutlet.ConnectTo(internalInlet, false);
            return adapterOutlet;
        }

        private void ConnectSharedResource(SharedResource otherSharedResource)
        {
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(SharedResource, otherSharedResource);
            resourceGroup.ConnectSharedResources(SharedResource, otherSharedResource);
            resourceGroup.FreeSharedResources();
        }

        protected override Action<TMessage> FindReceiverFor<TMessage>(IInlet<TMessage> inletSendingMessage)
        {
            var outlet = inletOutletBiLookup[inletSendingMessage].Single();
            var typedOutlet = (IOutlet<TMessage>) outlet;
            return typedOutlet.FindReceiver();
        }

        protected override Func<TMessage> FindSenderFor<TMessage>(IOutlet<TMessage> outletReceivingMessage)
        {
            var inlet = inletOutletBiLookup[outletReceivingMessage].Single();
            var typedInlet = (IInlet<TMessage>) inlet;
            return typedInlet.FindSender();
        }
    }
}
