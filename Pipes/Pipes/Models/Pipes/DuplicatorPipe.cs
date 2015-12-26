using System;
using Pipes.Constants;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IDuplicatorPipe<TMessage> : ITwoOutletPipe<TMessage>
    {
    }

    public class DuplicatorPipe<TMessage> : TwoOutletPipe<TMessage>, IDuplicatorPipe<TMessage>
    {
        // If the duplicator cannot pass the message down both outlets as it believed it could, it remembers which outlet must receive a message
        // with the variables below.
        private TMessage MessageWaitingToBeDuplicated { get; set; }
        private IOutlet<TMessage> OutletWaitingToReceiveDuplicatedMessage { get; set; }
        private bool HasMessageWaitingForDuplication => OutletWaitingToReceiveDuplicatedMessage != null;

        public DuplicatorPipe(IInlet<TMessage> inlet, IOutlet<TMessage> leftOutlet, IOutlet<TMessage> rightOutlet)
            : base(inlet, leftOutlet, rightOutlet)
        {
        }

        public override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            var leftReceiver = LeftOutlet.FindReceiver();
            var rightReceiver = RightOutlet.FindReceiver();

            if (leftReceiver == null) return null;
            if (rightReceiver == null) return null;

            return message =>
            {
                leftReceiver(message);
                var secondReceiver = RightOutlet.FindReceiver();
                if (secondReceiver == null)
                {
                    OutletWaitingToReceiveDuplicatedMessage = RightOutlet;
                    MessageWaitingToBeDuplicated = message;
                }
                else
                {
                    secondReceiver(message);
                }
            };
        }

        public override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            if (HasMessageWaitingForDuplication)
            {
                if (outletReceivingMessage == OutletWaitingToReceiveDuplicatedMessage)
                {
                    return () =>
                    {
                        OutletWaitingToReceiveDuplicatedMessage = null;
                        return MessageWaitingToBeDuplicated;
                    };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var sender = Inlet.FindSender();
                if (sender == null) return null;
                if (outletReceivingMessage == LeftOutlet)
                {
                    var secondReceiver = RightOutlet.FindReceiver();
                    if (secondReceiver == null) return null;

                    return () =>
                    {
                        var message = sender();

                        return message;
                    };
                }
            }
        }
    }
}