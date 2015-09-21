using System;
using System.Threading;

namespace Pipes.Models.Lets
{
    internal class WaitingSender<TMessage>
    {
        public readonly TMessage Message;
        public readonly Semaphore WaitSemaphore;

        public bool MessageSent { get; private set; }

        public WaitingSender(TMessage message)
        {
            Message = message;
            WaitSemaphore = new Semaphore(0, 1);
            MessageSent = false;
        }

        public void RecordMessageSent()
        {
            if (MessageSent) throw new InvalidOperationException("You cannot record a message as sent more than once");
            MessageSent = true;
        }
    }
}
