using System;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IEitherInletPipe<out TTieBreaker, TMessage> : ITwoInletPipe<TMessage> where TTieBreaker : ITieBreaker
    {
        TTieBreaker TieBreaker { get; }
    }

    public class EitherInletPipe<TTieBreaker, TMessage> : TwoInletPipe<TMessage>, IEitherInletPipe<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        public TTieBreaker TieBreaker { get; private set; }

        public EitherInletPipe(TTieBreaker tieBreaker)
        {
            TieBreaker = tieBreaker;
        }

        public override Action<TMessage> FindReceiver()
        {
            return Outlet.FindReceiver();
        }

        public override Func<TMessage> FindSender()
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