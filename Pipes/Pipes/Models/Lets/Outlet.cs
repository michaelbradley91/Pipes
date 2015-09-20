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
        protected internal Inlet<TMessage> ConnectedInlet { get; set; }

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
            if (ConnectedInlet != null)
            {
                Unlock();
                throw new InvalidOperationException("You cannot receive through a connected outlet.");
            }

            if (!HasWaitingReceiver())
            {
                var sender = Pipe.FindSender();
                if (sender != null)
                {
                    var message = sender();
                    Unlock();
                    return message;
                }
            }
            var waitingReceiver = new WaitingReceiver<TMessage>();
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

        public Action<TMessage> FindReceiver()
        {
            if (ConnectedInlet != null) return ConnectedInlet.Pipe.FindReceiver();
            if (HasWaitingReceiver()) return UseWaitingReceiver;
            return null;
        }

        private bool HasWaitingReceiver()
        {
            return waitingReceivers.Any();
        }

        private void UseWaitingReceiver(TMessage message)
        {
            var waitingReceiver = waitingReceivers.First();
            waitingReceivers.Remove(waitingReceiver);
            waitingReceiver.ReceiveMessage(message);
            waitingReceiver.WaitSemaphore.Release();
        }
    }
}