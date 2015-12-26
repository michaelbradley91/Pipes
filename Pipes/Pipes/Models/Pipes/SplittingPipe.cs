using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ISplittingPipe<TMessage> : ITwoOutletPipe<TMessage>
    {
    }

    public class SplittingPipe<TMessage> : TwoOutletPipe<TMessage>, ISplittingPipe<TMessage>
    {
        public SplittingPipe(IInlet<TMessage> inlet, IOutlet<TMessage> leftOutlet, IOutlet<TMessage> rightOutlet)
            : base(inlet, leftOutlet, rightOutlet)
        {
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            var leftReceiver = LeftOutlet.FindReceiver();
            var rightReceiver = RightOutlet.FindReceiver();

            if (leftReceiver == null) return null;
            if (rightReceiver == null) return null;

            return message =>
            {
                leftReceiver(message);
                rightReceiver(message);
            };
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            var sender = Inlet.FindSender();
            if (sender == null) return null;

            var otherReceiver = outletReceivingMessage == LeftOutlet ? RightOutlet.FindReceiver() : LeftOutlet.FindReceiver();
            if (otherReceiver == null) return null;

            return () =>
            {
                var message = sender();

                otherReceiver(message);

                return message;
            };
        }
    }
}