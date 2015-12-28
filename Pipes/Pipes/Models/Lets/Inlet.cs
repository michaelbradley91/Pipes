using System;

namespace Pipes.Models.Lets
{
    public interface IInlet : ILet
    {
        IOutlet TypelessConnectedOutlet { get; }
    }

    public interface IInlet<TMessage> : IInlet
    {
        /// <summary>
        /// Returns true if and only if this inlet can be connected to an outlet.
        /// This should only be called by a thread which has acquired this inlet's resource.
        /// This method must not intentionally throw an exception.
        /// </summary>
        bool CanConnect();

        /// <summary>
        /// The outlet this inlet is connected to. The setter should only be used in conjunction with other methods on this interface,
        /// and only when the outlet's resource has been acquired.
        /// </summary>
        IOutlet<TMessage> ConnectedOutlet { get; set; }
        
        /// <summary>
        /// Returns null if there is nothing ready to send a message down this inlet.
        /// This should only be called when the inlet's shared resource has been acquired.
        /// Evaluating this function will receive a message from a sender.
        /// </summary>
        Func<TMessage> FindSender();
    }
}