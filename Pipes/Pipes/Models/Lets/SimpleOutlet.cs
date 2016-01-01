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
    }

    public class SimpleOutlet<TMessage> : Outlet<TMessage>, ISimpleOutlet<TMessage>
    {
        private readonly IList<WaitingReceiver<TMessage>> waitingReceivers;

        public SimpleOutlet(Lazy<IPipe> pipe, SharedResource sharedResource) : base(pipe, sharedResource)
        {
            waitingReceivers = new List<WaitingReceiver<TMessage>>();
        }

        public TMessage Receive()
        {
            return Receive(s => s.WaitOne(), new ThreadInterruptedException("A message could not be received as the thread was interrupted"));
        }

        public TMessage Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), "The timespan cannot be negative");
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

        public override bool CanConnect()
        {
            return !waitingReceivers.Any() && ConnectedInlet == null;
        }
        
        public override Action<TMessage> FindReceiver()
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