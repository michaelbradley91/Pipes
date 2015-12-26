using System;
using System.Collections.Generic;
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
        /// The inlet should be one of the pipe's inlets.
        /// </summary>
        Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage);

        /// <summary>
        /// Find a sender on this pipe for the outlet that wishes to receive the message.
        /// 
        /// The outlet should be one of the pipe's outlets.
        /// </summary>
        Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage);
    }
}
