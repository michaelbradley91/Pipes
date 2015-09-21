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
        /// Retrieve a message from this pipe. If no message is available, this method will block until one arrives.
        /// </summary>
        public TMessage Receive()
        {
            return Receive(s => s.WaitOne(), new ThreadInterruptedException("A message could not be received as the thread was interrupted"));
        }

        /// <summary>
        /// Retrieve a message from this pipe. If no message is available, this will wait for up to approximately the 
        /// specified timeout to retrieve a message. If the timeout is exceeded, this will throw a timeout exception.
        /// </summary>
        public TMessage Receive(TimeSpan timeout)
        {
            if (timeout.CompareTo(TimeSpan.Zero) < 0) throw new ArgumentOutOfRangeException("timeout", "The timespan cannot be negative");
            return Receive(s => s.WaitOne(timeout), new TimeoutException("A message could not be received within the specified timeout"));
        }

        /// <summary>
        /// Retrieve a message from this pipe. If no message is available, this will throw an invalid operation exception.
        /// </summary>
        public TMessage ReceiveImmediately()
        {
            return Receive(s => s.WaitOne(0), new InvalidOperationException("A message could not be received immediately as the pipe system did not have a message ready"));
        }
        
        public TMessage Receive(Func<Semaphore, bool> waitFunction, Exception failureException)
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
            return WaitToReceiveMessage(waitingReceiver, waitFunction, failureException);
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
        /// Disconnect this outlet from its inlet.
        /// </summary>
        public void Disconnect()
        {
            if (ConnectedInlet == null) throw new InvalidOperationException("You cannot disconnect an outlet unless it is already connected");
            LockWith(ConnectedInlet);
            Disconnect(ConnectedInlet, this);
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