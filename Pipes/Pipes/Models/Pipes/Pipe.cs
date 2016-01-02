using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IPipe
    {
        /// <summary>
        /// All inlets that are public facing and thus can be connected to.
        /// These are used to compute the graph of the pipe system. Any inlet not included here will not be included in the graph.
        /// </summary>
        IReadOnlyCollection<IInlet> ConnectableInlets { get; }

        /// <summary>
        /// All outlets that are public facing and thus can be connected to.
        /// These are used to compute the graph of the pipe system. Any outlet not included here will not be included in the graph.
        /// </summary>
        IReadOnlyCollection<IOutlet> ConnectableOutlets { get; }

        /// <summary>
        /// Find an receiver on this pipe for the inlet that wishes to send the message.
        /// 
        /// The inlet should be one of the pipe's inlets. If it is not, this will throw an InvalidOperation exception.
        /// </summary>
        Action<TMessage> FindReceiver<TMessage>(IInlet<TMessage> inletSendingMessage);

        /// <summary>
        /// Find a sender on this pipe for the outlet that wishes to receive the message.
        /// 
        /// The outlet should be one of the pipe's outlets. If it is not, this will throw an InvalidOperation exception.
        /// </summary>
        Func<TMessage> FindSender<TMessage>(IOutlet<TMessage> outletReceivingMessage);

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
        public IReadOnlyCollection<IInlet> ConnectableInlets { get; }
        public IReadOnlyCollection<IOutlet> ConnectableOutlets { get; }
        public SharedResource SharedResource { get; }

        /// <summary>
        /// A collection of all inlets associated to this pipe.
        /// (Therefore, these inlets may ask the pipe for a receiver.)
        /// </summary>
        protected virtual IReadOnlyCollection<IInlet> PipeInlets => ConnectableInlets;

        /// <summary>
        /// A collection of all outlets associated to this pipe.
        /// (Therefore, these outlets may ask the pipe for a sender.)
        /// </summary>
        protected virtual IReadOnlyCollection<IOutlet> PipeOutlets => ConnectableOutlets;

        protected Pipe(IReadOnlyCollection<IInlet> connectableInlets, IReadOnlyCollection<IOutlet> connectableOutlets)
        {
            var allLetSharedResources = connectableInlets.Select(i => i.SharedResource).Concat(connectableOutlets.Select(o => o.SharedResource)).ToArray();
            SharedResource = SharedResource.CreateAndConnect(allLetSharedResources);
            ConnectableInlets = connectableInlets;
            ConnectableOutlets = connectableOutlets;
        }

        public Action<TMessage> FindReceiver<TMessage>(IInlet<TMessage> inletSendingMessage)
        {
            if (!PipeInlets.Contains(inletSendingMessage))
            {
                throw new InvalidOperationException("The inlet sending the message is not associated to this pipe.");
            }
            return FindReceiverFor(inletSendingMessage);
        }

        public Func<TMessage> FindSender<TMessage>(IOutlet<TMessage> outletReceivingMessage)
        {
            if (!PipeOutlets.Contains(outletReceivingMessage))
            {
                throw new InvalidOperationException("The outlet receiving the message is not associated to this pipe.");
            }
            return FindSenderFor(outletReceivingMessage);
        }

        /// <summary>
        /// An inlet passed in here will be a member of PipeInlets
        /// </summary>
        protected abstract Action<TMessage> FindReceiverFor<TMessage>(IInlet<TMessage> inletSendingMessage);

        /// <summary>
        /// An outlet passed in here will be a member of PipeOutlets
        /// </summary>
        protected abstract Func<TMessage> FindSenderFor<TMessage>(IOutlet<TMessage> outletReceivingMessage);
    }
}
