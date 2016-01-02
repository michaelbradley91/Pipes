using System;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

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

        /// <summary>
        /// Provides a sender casting TMessage to TTarget for convenience.
        /// </summary>
        Func<TTarget> FindSender<TTarget>();

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, this will also check to see if the pipe system would no longer be a tree after this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// 
        /// (This method will also connect the outlet to this inlet)
        /// </summary>
        void ConnectTo(IOutlet<TMessage> outlet, bool checkPipeSystemFormsTree = true);

        /// <summary>
        /// Disconnect this inlet from its connected outlet.
        /// 
        /// (This method will also disconnect the outlet from this inlet)
        /// </summary>
        void Disconnect();
    }

    public abstract class Inlet<TMessage> : Let, IInlet<TMessage>
    {
        public IOutlet<TMessage> ConnectedOutlet { get; set; }
        public IOutlet TypelessConnectedOutlet => ConnectedOutlet;

        protected Inlet(IPromised<IPipe> promisedPipe) : base(promisedPipe)
        {
            ConnectedOutlet = null;
        }

        public void ConnectTo(IOutlet<TMessage> outlet, bool checkPipeSystemFormsTree = true)
        {
            LockWith(outlet);
            Connect(this, outlet, checkPipeSystemFormsTree);
            Unlock();
        }

        public void Disconnect()
        {
            if (ConnectedOutlet == null) throw new InvalidOperationException("You cannot disconnect an inlet unless it is already connected");
            LockWith(ConnectedOutlet);
            Disconnect(this, ConnectedOutlet);
            Unlock();
        }

        public abstract bool CanConnect();

        public abstract Func<TMessage> FindSender();

        public Func<TTarget> FindSender<TTarget>()
        {
            var sender = FindSender();
            if (sender == null) return null;
            return () => (TTarget)(object)sender();
        }
    }
}