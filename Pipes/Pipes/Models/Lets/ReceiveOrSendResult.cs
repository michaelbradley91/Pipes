using System;

namespace Pipes.Models.Lets
{
    public class ReceiveOrSendResult<TReceive, TSend>
    {
        public bool MessageReceived => !MessageSent;
        public bool MessageSent { get; }

        private readonly TReceive receivedMessage;
        
        private ReceiveOrSendResult(TReceive receivedMessage)
        {
            MessageSent = false;
            this.receivedMessage = receivedMessage;
        }

        private ReceiveOrSendResult()
        {
            MessageSent = true;
        }

        public TReceive GetReceivedMessage()
        {
            if (MessageSent) throw new InvalidOperationException();
            return receivedMessage;
        }
        
        public static ReceiveOrSendResult<TReceive, TSend> CreateSendResult()
        {
            return new ReceiveOrSendResult<TReceive, TSend>();
        }

        public static ReceiveOrSendResult<TReceive, TSend> CreateReceiveResult(TReceive receivedMessage)
        {
            return new ReceiveOrSendResult<TReceive, TSend>(receivedMessage);
        }
    }
}
