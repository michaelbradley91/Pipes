using System;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IEitherOutletPipe<out TTieBreaker, TMessage> : ITwoOutletPipe<TMessage> where TTieBreaker : ITieBreaker
    {
        TTieBreaker TieBreaker { get; }
    }

    public class EitherOutletPipe<TTieBreaker, TMessage> : TwoOutletPipe<TMessage>, IEitherOutletPipe<TTieBreaker, TMessage> where TTieBreaker : ITieBreaker
    {
        public TTieBreaker TieBreaker { get; private set; }

        protected EitherOutletPipe(TTieBreaker tieBreaker)
        {
            TieBreaker = tieBreaker;
        }

        public override Action<TMessage> FindReceiver()
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

        public override Func<TMessage> FindSender()
        {
            return Inlet.FindSender();
        }
    }
}