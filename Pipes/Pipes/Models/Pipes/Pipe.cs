using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IPipe
    {
        IReadOnlyCollection<IInlet> AllInlets { get; }
        IReadOnlyCollection<IOutlet> AllOutlets { get; }

        /// <summary>
        /// Find an receiver on this pipe for the inlet that wishes to send the message.
        /// 
        /// The inlet should be one of the pipe's inlets. If it is not and checkInlet is true (its default), this will throw an InvalidOperation exception.
        /// If checkInlet is false, no exception will be thrown but the pipe may not behave as expected.
        /// </summary>
        Action<TMessage> FindReceiver<TMessage>(IInlet<TMessage> inletSendingMessage, bool checkInlet = true);

        /// <summary>
        /// Find a sender on this pipe for the outlet that wishes to receive the message.
        /// 
        /// The outlet should be one of the pipe's outlets. If it is not and checkOutlet is true (its default), this will throw an InvalidOperation exception.
        /// If checkOutlet is false, no exception will be thrown but the pipe may not behave as expected.
        /// </summary>
        Func<TMessage> FindSender<TMessage>(IOutlet<TMessage> outletReceivingMessage, bool checkOutlet = true);

        /// <summary>
        /// This is a technical field. This is the shared resource that is associated to this pipe. The inlets and outlets
        /// are then connected to its shared resource forming a common resource group.
        /// 
        /// In short, acquiring this shared resource acquires all components of the pipe system this pipe is connected to.
        /// 
        /// You normally should not require access to this unless using existing pipes to form your own custom pipes.
        /// </summary>
        SharedResource SharedResource { get; }
    }

    public abstract class Pipe : IPipe
    {
        public IReadOnlyCollection<IInlet> AllInlets { get; }
        public IReadOnlyCollection<IOutlet> AllOutlets { get; }
        public SharedResource SharedResource { get; }

        protected Pipe(IReadOnlyCollection<IInlet> allInlets, IReadOnlyCollection<IOutlet> allOutlets)
        {
            var allLetSharedResources = allInlets.Select(i => i.SharedResource).Concat(allOutlets.Select(o => o.SharedResource)).ToArray();

            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(allLetSharedResources);
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            foreach (var letSharedResource in allLetSharedResources)
            {
                resourceGroup.ConnectSharedResources(letSharedResource, pipeResource);
            }

            resourceGroup.FreeSharedResources();

            AllInlets = allInlets;
            AllOutlets = allOutlets;
            SharedResource = pipeResource;
        }

        public Action<TMessage> FindReceiver<TMessage>(IInlet<TMessage> inletSendingMessage, bool checkInlet)
        {
            if (checkInlet && !AllInlets.Contains(inletSendingMessage))
            {
                throw new InvalidOperationException("The inlet sending the message is not associated to this pipe.");
            }

            return FindReceiverFor(inletSendingMessage);
        }

        public Func<TMessage> FindSender<TMessage>(IOutlet<TMessage> outletReceivingMessage, bool checkOutlet)
        {
            if (checkOutlet && !AllOutlets.Contains(outletReceivingMessage))
            {
                throw new InvalidOperationException("The outlet receiving the message is not associated to this pipe.");
            }

            return FindSenderFor(outletReceivingMessage);
        }

        /// <summary>
        /// There is no need to check if the inlet is one of yours here, as this has been handled by the base class (if requested).
        /// </summary>
        protected abstract Action<TMessage> FindReceiverFor<TMessage>(IInlet<TMessage> inletSendingMessage);

        /// <summary>
        /// There is no need to check if the outlet is one of yours here, as this has been handled by the base class (if requested).
        /// </summary>
        protected abstract Func<TMessage> FindSenderFor<TMessage>(IOutlet<TMessage> outletReceivingMessage);
    }
}
