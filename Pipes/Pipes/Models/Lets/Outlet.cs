using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public class Outlet<TMessage> : Let<TMessage>
    {
        internal Inlet<TMessage> ConnectedInlet;

        private readonly IList<WaitingReceiver<TMessage>> waitingReceivers; 

        internal Outlet(IPipe<TMessage> pipe, SharedResource resource) : base(pipe, resource)
        {
            waitingReceivers = new List<WaitingReceiver<TMessage>>();
            ConnectedInlet = null;
        }

        /// <summary>
        /// Retrieve a message from this outlet. If no message is available, this method will block until one arrives.
        /// </summary>
        public TMessage Receive()
        {
            Lock();
            var waitingReceiver = new WaitingReceiver<TMessage>();
            if (!waitingReceivers.Any()) Pipe.TryToReceive(waitingReceiver);
            if (waitingReceiver.MessageReceived)
            {
                Unlock();
                return waitingReceiver.GetMessage();
            }
            waitingReceivers.Add(waitingReceiver);
            Unlock();
            return WaitToReceiveMessage(waitingReceiver, s => s.WaitOne(), new ThreadInterruptedException("A message could not be received as the thread was interrupted"));
        }

        private TMessage WaitToReceiveMessage(WaitingReceiver<TMessage> waitingReceiver, Func<Semaphore, bool> waitFunction, Exception failureException)
        {
            try
            {
                var messageReceived = waitFunction(waitingReceiver.WaitSemaphore);
                if (messageReceived) return waitingReceiver.GetMessage();
                Lock();
                if (!waitingReceiver.MessageReceived)
                {
                    waitingReceivers.Remove(waitingReceiver);
                    Unlock();
                    throw failureException;
                } // else a message has actually been received (a moment later)
                Unlock();
                return waitingReceiver.GetMessage();
            }
            catch
            {
                Lock();
                if (!waitingReceiver.MessageReceived) waitingReceivers.Remove(waitingReceiver);
                Unlock();
                throw;
            }
        }

        /// <summary>
        /// Connect this outlet to an inlet. This helps you to build up a pipe system!
        /// By default, the method will also check to see if you would create a cycle by doing this. If so, it will refuse to connect to the given inlet and throw
        /// an InvalidOperationException. This is quite an expensive check for large pipe systems however, so if you're confident you are not creating cycles, you
        /// can turn it off.
        /// </summary>
        public void ConnectTo(Inlet<TMessage> inlet, bool checkForCycles = true)
        {
            LockWith(inlet);
            Connect(inlet, this, checkForCycles);
            Unlock();
        }

        /// <summary>
        /// Disconnect this outlet from the given inlet. This breaks a pipe system apart.
        /// </summary>
        public void DisconnectFrom(Inlet<TMessage> inlet)
        {
            LockWith(inlet);
            Disconnect(inlet, this);
            Unlock();
        }

        protected override bool ReadyToConnect()
        {
            return !waitingReceivers.Any() && ConnectedInlet == null;
        }
    }
}