using System;

namespace Pipes.Models.Lets
{
    public interface IOutlet : ILet
    {
        IInlet TypelessConnectedInlet { get; }
    }

    public interface IOutlet<TMessage> : IOutlet
    {
        /// <summary>
        /// Returns true if and only if this outlet can be connected to an inlet.
        /// This should only be called by a thread which has acquired this outlet's resource.
        /// This method must not intentionally throw an exception.
        /// </summary>
        bool CanConnect();

        /// <summary>
        /// The inlet this outlet is connected to. The setter should only be used in conjunction with other methods on this interface,
        /// and only when the outlet's resource has been acquired.
        /// </summary>
        IInlet<TMessage> ConnectedInlet { get; set; }
        
        /// <summary>
        /// Returns null if there is nothing ready to receive a message from this outlet.
        /// This should only be called when the outlet's shared resource has been acquired.
        /// Evaluating this action will send a message to a the receiver.
        /// </summary>
        Action<TMessage> FindReceiver();
    }
}