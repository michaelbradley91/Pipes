using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IAlternatingTwoWayTieBreaker : ITwoWayTieBreaker
    {
        Priority InitialPriority { get; }
        Priority NextPriority { get; }
    }

    public class AlternatingTwoWayTieBreaker : TwoWayTieBreaker, IAlternatingTwoWayTieBreaker
    {
        public Priority InitialPriority { get; }
        public Priority NextPriority { get; private set; }

        public AlternatingTwoWayTieBreaker(Alternated alternated)
        {
            switch (alternated)
            {
                case Alternated.LeftHasPriorityInitially:
                    InitialPriority = Priority.Left;
                    break;
                case Alternated.RightHasPriorityInitially:
                    InitialPriority = Priority.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alternated));
            }
            NextPriority = InitialPriority;
        }

        public override TieResult ResolveTie()
        {
            switch (NextPriority)
            {
                case Priority.Left:
                    NextPriority = Priority.Right;
                    return TieResult.Left;
                case Priority.Right:
                    NextPriority = Priority.Left;
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
