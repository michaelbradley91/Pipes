using System;
using System.Diagnostics;

namespace Pipes.Models.Lets
{
    public interface IValve<TReceive, TSend>
    {
        /// <summary>
        /// Send a message down the pipe or receive a message. This will block until either your message is sent, or you have received a message.
        /// (The message you were going to send is then thrown away).
        /// </summary>
        ReceiveOrSendResult<TReceive, TSend> ReceiveOrSend(TSend message);

        /// <summary>
        /// Send a message down the pipe or receive a message. This will wait for up to approximately the 
        /// specified timeout to either your send the message or receive a message. If the timeout is exceeded, this will throw a timeout exception.
        /// </summary>
        ReceiveOrSendResult<TReceive, TSend> ReceiveOrSend(TSend message, TimeSpan timeout);

        /// <summary>
        /// Send a message down the pipe or receive a message. If the pipe system cannot send your message immediately, or return a message to you immediately,
        /// this will throw an invalid operation exception.
        /// </summary>
        ReceiveOrSendResult<TReceive, TSend> ReceiveOrSendImmediately(TSend message);
    }

    public class Valve<TReceive, TSend> : IValve<TReceive, TSend>
    {
        private readonly ISimpleInlet<TSend> preparationInlet;
        private readonly ISimpleOutlet<TSend> flushOutlet;
        private readonly ISimpleOutlet<ReceiveOrSendResult<TReceive, TSend>> resultOutlet;

        /// <summary>
        /// The preparation inlet must accept the message which could be sent. It should only hold one unsent message at a time.
        /// The flush outlet must output and empty the messages prepared to be sent.
        /// The result outlet should return the result - if the message was sent or if we received a message.
        /// </summary>
        public Valve(ISimpleInlet<TSend> preparationInlet, ISimpleOutlet<TSend> flushOutlet, ISimpleOutlet<ReceiveOrSendResult<TReceive, TSend>> resultOutlet)
        {
            this.preparationInlet = preparationInlet;
            this.flushOutlet = flushOutlet;
            this.resultOutlet = resultOutlet;
        }

        public ReceiveOrSendResult<TReceive, TSend> ReceiveOrSend(TSend message)
        {
            preparationInlet.Send(message);
            return TryButFlushOnException(() => resultOutlet.Receive());
        }

        public ReceiveOrSendResult<TReceive, TSend> ReceiveOrSend(TSend message, TimeSpan timeout)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            preparationInlet.Send(message, timeout);
            stopWatch.Stop();

            var timeRemaining = timeout.Subtract(stopWatch.Elapsed);
            if (timeRemaining < TimeSpan.Zero) timeRemaining = TimeSpan.FromMilliseconds(1);

            return TryButFlushOnException(() => resultOutlet.Receive(timeRemaining));
        }

        public ReceiveOrSendResult<TReceive, TSend> ReceiveOrSendImmediately(TSend message)
        {
            preparationInlet.SendImmediately(message);
            return TryButFlushOnException(() => resultOutlet.ReceiveImmediately());
        }

        private ReceiveOrSendResult<TReceive, TSend> TryButFlushOnException(Func<ReceiveOrSendResult<TReceive, TSend>> receive)
        {
            try
            {
                var result = receive();
                if (!result.MessageSent)
                {
                    flushOutlet.ReceiveImmediately();
                }
                return result;
            }
            catch
            {
                flushOutlet.ReceiveImmediately();
                throw;
            }
        }
    }
}
