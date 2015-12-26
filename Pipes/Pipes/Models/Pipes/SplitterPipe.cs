using System;
using Pipes.Constants;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ISplitterPipe<TMessage> : ITwoOutletPipe<TMessage>
    {
    }

    public class SplitterPipe<TMessage> : TwoOutletPipe<TMessage>, ISplitterPipe<TMessage>
    {
        public SplitterPipe(IInlet<TMessage> inlet, IOutlet<TMessage> leftOutlet, IOutlet<TMessage> rightOutlet)
            : base(inlet, leftOutlet, rightOutlet)
        {
        }

        public override Action<TMessage> FindReceiver()
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

        public override Func<TMessage> FindSender()
        {

            return Inlet.FindSender();
        }
    }
}