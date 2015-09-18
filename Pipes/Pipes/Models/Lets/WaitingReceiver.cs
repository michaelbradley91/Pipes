using System;
using System.Threading;

namespace Pipes.Models.Lets
{
    internal class WaitingReceiver<TMessage>
    {
        public readonly Semaphore WaitSemaphore;

        public bool MessageReceived { get; private set; }
        private TMessage message;

        public WaitingReceiver()
        {
            message = default(TMessage);
            MessageReceived = false;
            WaitSemaphore = new Semaphore(0, 1);
        }

        public void ReceiveMessage(TMessage messageToReceive)
        {
            if (MessageReceived) throw new InvalidOperationException("Cannot receive more than one message.");
            message = messageToReceive;
            MessageReceived = true;
        }

        public TMessage GetMessage()
        {
            if (!MessageReceived) throw new InvalidOperationException("Cannot get the message before it has been received.");
            return message;
        }
    }
}
