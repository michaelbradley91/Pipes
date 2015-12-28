﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public interface ISimpleInlet<TMessage> : IInlet<TMessage>
    {
        /// <summary>
        /// Send a message down the pipe. If the pipe system has insufficient capacity to accept the message, this will block until the message can be sent.
        /// </summary>
        void Send(TMessage message);

        /// <summary>
        /// Send a message down the pipe. If the pipe system has insufficient capacity to accept the message, this will wait for up to approximately the 
        /// specified timeout to send the message. If the timeout is exceeded, this will throw a timeout exception.
        /// </summary>
        void Send(TMessage message, TimeSpan timeout);

        /// <summary>
        /// Send a message down the pipe. If the pipe system has insufficient capacity to accept the message, this will throw an invalid operation exception.
        /// </summary>
        void SendImmediately(TMessage message);

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

    public class SimpleInlet<TMessage> : Let, ISimpleInlet<TMessage>
    {
        public IOutlet<TMessage> ConnectedOutlet { get; set; }
        public IOutlet TypelessConnectedOutlet => ConnectedOutlet;

        private readonly IList<WaitingSender<TMessage>> waitingSenders;

        internal SimpleInlet(Lazy<IPipe> pipe, SharedResource sharedResource) : base(pipe, sharedResource)
        {
            waitingSenders = new List<WaitingSender<TMessage>>();
            ConnectedOutlet = null;
        }
        public void Send(TMessage message)
        {
            Send(message, s => s.WaitOne(), new ThreadInterruptedException("The message could not be sent as the thread was interrupted"));
        }

        public void Send(TMessage message, TimeSpan timeout)
        {
            if (timeout.CompareTo(TimeSpan.Zero) < 0) throw new ArgumentOutOfRangeException(nameof(timeout), "The timespan cannot be negative");
            Send(message, s => s.WaitOne(timeout), new TimeoutException("The message could not be sent within the specified timeout"));
        }

        public void SendImmediately(TMessage message)
        {
            Send(message, s => s.WaitOne(0), new InvalidOperationException("The message could not be sent immediately as the pipe system was not ready to receive a message"));
        }

        private void Send(TMessage message, Func<Semaphore, bool> waitFunction, Exception failureException)
        {
            Lock();
            if (ConnectedOutlet != null)
            {
                Unlock();
                throw new InvalidOperationException("You cannot send through a connected inlet.");
            }

            if (!HasWaitingSender())
            {
                var receiver = Pipe.FindReceiver(this);
                if (receiver != null)
                {
                    receiver(message);
                    Unlock();
                    return;
                }
            }
            var waitingSender = new WaitingSender<TMessage>(message);
            waitingSenders.Add(waitingSender);
            Unlock();
            WaitToSendMessage(waitingSender, waitFunction, failureException);
        }

        private void WaitToSendMessage(WaitingSender<TMessage> waitingSender, Func<Semaphore, bool> waitFunction, Exception failureException)
        {
            try
            {
                var messageSent = waitFunction(waitingSender.WaitSemaphore);
                if (messageSent) return;
                Lock();
                if (!waitingSender.MessageSent)
                {
                    waitingSenders.Remove(waitingSender);
                    Unlock();
                    throw failureException;
                } // else the message has actually been sent (a moment later)
                Unlock();
            }
            catch
            {
                Lock();
                if (!waitingSender.MessageSent) waitingSenders.Remove(waitingSender);
                Unlock();
                throw;
            }
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

        public bool CanConnect()
        {
            return !waitingSenders.Any() && ConnectedOutlet == null;
        }

        public Func<TMessage> FindSender()
        {
            if (ConnectedOutlet != null) return ConnectedOutlet.Pipe.FindSender(ConnectedOutlet);
            if (HasWaitingSender()) return UseWaitingSender;
            return null;
        }

        private bool HasWaitingSender()
        {
            return waitingSenders.Any();
        }

        private TMessage UseWaitingSender()
        {
            var waitingSender = waitingSenders.First();
            waitingSenders.Remove(waitingSender);
            waitingSender.RecordMessageSent();
            waitingSender.WaitSemaphore.Release();
            return waitingSender.Message;
        }
    }
}