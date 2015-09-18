using System;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public class Inlet<TMessage> : Let<TMessage>
    {
        internal Outlet<TMessage> ConnectedOutlet; 

        internal Inlet(IPipe<TMessage> pipe, SharedResource resource) : base(pipe, resource)
        {
            ConnectedOutlet = null;
        }

        /// <summary>
        /// Send a message down the pipe. If the pipe system has insufficient capacity to accept the message, this will block until the message can be sent.
        /// </summary>
        public void Send(TMessage message)
        {
            Lock();
            // TODO RESOLVE THE PIPE SYSTEM
            Unlock();
        }

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, the method will also check to see if you would create a cycle by doing this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// </summary>
        public void ConnectTo(Outlet<TMessage> outlet, bool checkForCycles = true)
        {
            LockWith(outlet);
            Connect(this, outlet, checkForCycles);
            Unlock();
        }

        /// <summary>
        /// Disconnect this outlet from the given inlet. This breaks a pipe system apart.
        /// </summary>
        public void DisconnectFrom(Outlet<TMessage> outlet)
        {
            LockWith(outlet);
            Disconnect(this, outlet);
            Unlock();
        }

        protected override bool ReadyToConnect()
        {
            throw new NotImplementedException();
        }
    }
}