using System;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

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

        /// <summary>
        /// Provides a receiver casting TMessage to TTarget for convenience.
        /// </summary>
        Action<TTarget> FindReceiver<TTarget>();

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, this will also check to see if the pipe system would no longer be a tree after this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// 
        /// (This method will also connect the outlet to this inlet)
        /// </summary>
        void ConnectTo(IInlet<TMessage> inlet, bool checkPipeSystemFormsTree = true);

        /// <summary>
        /// Disconnect this outlet from its inlet.
        /// 
        /// (This method will also disconnect the outlet from this inlet)
        /// </summary>
        void Disconnect();
    }

    public abstract class Outlet<TMessage> : Let, IOutlet<TMessage>
    {
        public IInlet<TMessage> ConnectedInlet { get; set; }
        public IInlet TypelessConnectedInlet => ConnectedInlet;

        protected Outlet(IPromised<IPipe> promisedPipe) : base(promisedPipe)
        {
            ConnectedInlet = null;
        }

        public void ConnectTo(IInlet<TMessage> inlet, bool checkPipeSystemFormsTree = true)
        {
            LockWith(inlet);
            Connect(inlet, this, checkPipeSystemFormsTree);
            Unlock();
        }
        
        public void Disconnect()
        {
            if (ConnectedInlet == null) throw new InvalidOperationException("You cannot disconnect an outlet unless it is already connected");
            LockWith(ConnectedInlet);
            Disconnect(ConnectedInlet, this);
            Unlock();
        }

        public abstract bool CanConnect();

        public abstract Action<TMessage> FindReceiver();

        public Action<TTarget> FindReceiver<TTarget>()
        {
            var receiver = FindReceiver();
            if (receiver == null) return null;
            return m => receiver((TMessage)(object)m);
        }
    }
}