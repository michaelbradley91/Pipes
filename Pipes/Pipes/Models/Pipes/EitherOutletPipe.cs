using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IEitherOutletPipe<out TTieBreaker, TMessage> : ITwoOutletPipe<TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        TTieBreaker TieBreaker { get; }
    }

    public class EitherOutletPipe<TTieBreaker, TMessage> : TwoOutletPipe<TMessage>, IEitherOutletPipe<TTieBreaker, TMessage> where TTieBreaker : ITwoWayTieBreaker
    {
        public TTieBreaker TieBreaker { get; }

        public EitherOutletPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> leftOutlet, ISimpleOutlet<TMessage> rightOutlet, TTieBreaker tieBreaker)
            : base(inlet, leftOutlet, rightOutlet)
        {
            TieBreaker = tieBreaker;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            var leftReceiver = LeftOutlet.FindReceiver();
            var rightReceiver = RightOutlet.FindReceiver();

            if (leftReceiver == null) return rightReceiver;
            if (rightReceiver == null) return leftReceiver;

            switch (TieBreaker.ResolveTie())
            {
                case TieResult.Left:
                    return leftReceiver;
                case TieResult.Right:
                    return rightReceiver;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return Inlet.FindSender();
        }
    }
}