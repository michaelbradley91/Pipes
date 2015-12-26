using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IPipe<TMessage>
    {
        IReadOnlyCollection<IInlet<TMessage>> Inlets { get; }
        IReadOnlyCollection<IOutlet<TMessage>> Outlets { get; }

        /// <summary>
        /// Find an receiver on this pipe for the inlet that wishes to send the message.
        /// 
        /// The inlet should be one of the pipe's inlets. If it is not and checkInlet is true (its default), this will throw an InvalidOperation exception.
        /// If checkInlet is false, no exception will be thrown but the pipe may not behave as expected.
        /// </summary>
        Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage, bool checkInlet = true);

        /// <summary>
        /// Find a sender on this pipe for the outlet that wishes to receive the message.
        /// 
        /// The outlet should be one of the pipe's outlets. If it is not and checkOutlet is true (its default), this will throw an InvalidOperation exception.
        /// If checkOutlet is false, no exception will be thrown but the pipe may not behave as expected.
        /// </summary>
        Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage, bool checkOutlet = true);
    }

    public abstract class Pipe<TMessage> : IPipe<TMessage>
    {
        public abstract IReadOnlyCollection<IInlet<TMessage>> Inlets { get; }
        public abstract IReadOnlyCollection<IOutlet<TMessage>> Outlets { get; }

        public Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage, bool checkInlet)
        {
            if (checkInlet && !Inlets.Contains(inletSendingMessage))
            {
                throw new InvalidOperationException("The inlet sending the message is not associated to this pipe.");
            }

            return FindReceiver(inletSendingMessage);
        }

        public Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage, bool checkOutlet)
        {
            if (checkOutlet && !Outlets.Contains(outletReceivingMessage))
            {
                throw new InvalidOperationException("The outlet receiving the message is not associated to this pipe.");
            }

            return FindSender(outletReceivingMessage);
        }

        /// <summary>
        /// There is no need to check if the inlet is one of yours here, as this has been handled by the base class (if requested).
        /// </summary>
        protected abstract Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage);

        /// <summary>
        /// There is no need to check if the outlet is one of yours here, as this has been handled by the base class (if requested).
        /// </summary>
        protected abstract Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage);
    }
}
