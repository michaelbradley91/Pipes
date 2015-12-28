using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public interface ISimpleOutlet<TMessage> : IOutlet<TMessage>
    {
        /// <summary>
        /// Retrieve a message from this pipe. If no message is available, this method will block until one arrives.
        /// </summary>
        TMessage Receive();

        /// <summary>
        /// Retrieve a message from this pipe. If no message is available, this will wait for up to approximately the 
        /// specified timeout to retrieve a message. If the timeout is exceeded, this will throw a timeout exception.
        /// </summary>
        TMessage Receive(TimeSpan timeout);

        /// <summary>
        /// Retrieve a message from this pipe. If no message is available, this will throw an invalid operation exception.
        /// </summary>
        TMessage ReceiveImmediately();

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

    public class SimpleOutlet<TMessage> : Let, ISimpleOutlet<TMessage>
    {
        public IInlet<TMessage> ConnectedInlet { get; set; }
        public IInlet TypelessConnectedInlet => ConnectedInlet; 

        private readonly IList<WaitingReceiver<TMessage>> waitingReceivers;

        internal SimpleOutlet(Lazy<IPipe> pipe, SharedResource sharedResource) : base(pipe, sharedResource)
        {
            waitingReceivers = new List<WaitingReceiver<TMessage>>();
            ConnectedInlet = null;
        }

        public TMessage Receive()
        {
            return Receive(s => s.WaitOne(), new ThreadInterruptedException("A message could not be received as the thread was interrupted"));
        }

        public TMessage Receive(TimeSpan timeout)
        {
            if (timeout.CompareTo(TimeSpan.Zero) < 0) throw new ArgumentOutOfRangeException(nameof(timeout), "The timespan cannot be negative");
            return Receive(s => s.WaitOne(timeout), new TimeoutException("A message could not be received within the specified timeout"));
        }

        public TMessage ReceiveImmediately()
        {
            return Receive(s => s.WaitOne(0), new InvalidOperationException("A message could not be received immediately as the pipe system did not have a message ready"));
        }
        
        private TMessage Receive(Func<Semaphore, bool> waitFunction, Exception failureException)
        {
            Lock();
            if (ConnectedInlet != null)
            {
                Unlock();
                throw new InvalidOperationException("You cannot receive through a connected outlet.");
            }

            if (!HasWaitingReceiver())
            {
                var sender = Pipe.FindSender(this);
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

        public void ConnectTo(IInlet<TMessage> inlet, bool checkPipeSystemFormsTree = true)
        {
            LockWith(inlet);
            Connect(inlet, this, checkPipeSystemFormsTree);
            Unlock();
        }

        public bool CanConnect()
        {
            return !waitingReceivers.Any() && ConnectedInlet == null;
        }

        public void Disconnect()
        {
            if (ConnectedInlet == null) throw new InvalidOperationException("You cannot disconnect an outlet unless it is already connected");
            LockWith(ConnectedInlet);
            Disconnect(ConnectedInlet, this);
            Unlock();
        }

        public Action<TMessage> FindReceiver()
        {
            if (ConnectedInlet != null) return ConnectedInlet.Pipe.FindReceiver(ConnectedInlet);
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