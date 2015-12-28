using System;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IEitherInletPipe<out TTieBreaker, TMessage> : ITwoInletPipe<TMessage> where TTieBreaker : ITieBreaker
    {
        TTieBreaker TieBreaker { get; }
    }

    public class EitherInletPipe<TTieBreaker, TMessage> : TwoInletPipe<TMessage>, IEitherInletPipe<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        public TTieBreaker TieBreaker { get; }

        public EitherInletPipe(ISimpleInlet<TMessage> leftInlet, ISimpleInlet<TMessage> rightInlet, ISimpleOutlet<TMessage> outlet, TTieBreaker tieBreaker)
            : base(leftInlet, rightInlet, outlet)
        {
            TieBreaker = tieBreaker;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return Outlet.FindReceiver();
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            var leftSender = LeftInlet.FindSender();
            var rightSender = RightInlet.FindSender();

            if (leftSender == null) return rightSender;
            if (rightSender == null) return leftSender;

            switch (TieBreaker.ResolveTie())
            {
                case TieResult.Left:
                    return leftSender;
                case TieResult.Right:
                    return rightSender;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}