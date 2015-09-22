using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IAlternatingTieBreaker : ITieBreaker
    {
        Priority InitialPriority { get; }
        Priority NextPriority { get; }
    }

    public class AlternatingTieBreaker : IAlternatingTieBreaker
    {
        public Priority InitialPriority { get; private set; }
        public Priority NextPriority { get; private set; }

        public AlternatingTieBreaker(Alternated alternated)
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
                    throw new ArgumentOutOfRangeException("alternated");
            }
            NextPriority = InitialPriority;
        }

        public TieResult ResolveTie()
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
