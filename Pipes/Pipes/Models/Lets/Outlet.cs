using System;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public class Outlet<TMessageType> : Let<TMessageType>
    {
        internal Inlet<TMessageType> ConnectedInlet; 

        internal Outlet(IPipe<TMessageType> pipe, SharedResource resource) : base(pipe, resource)
        {
            ConnectedInlet = null;
        }

        /// <summary>
        /// Retrieve a message from this outlet. If no message is available, this method will block until one arrives.
        /// </summary>
        public TMessageType Receive()
        {
            Lock();
            // TODO RESOLVE THE PIPE SYSTEM
            Unlock();
            return default(TMessageType);
        }

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, the method will also check to see if you would create a cycle by doing this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// </summary>
        public void ConnectTo(Inlet<TMessageType> inlet, bool checkForCycles = true)
        {
            LockWith(inlet);
            Connect(inlet, this, checkForCycles);
            Unlock();
        }

        /// <summary>
        /// Disconnect this outlet from the given inlet. This breaks a pipe system apart.
        /// </summary>
        public void DisconnectFrom(Inlet<TMessageType> inlet)
        {
            LockWith(inlet);
            Disconnect(inlet, this);
            Unlock();
        }

        protected override bool ReadyToConnect()
        {
            throw new NotImplementedException();
        }
    }
}