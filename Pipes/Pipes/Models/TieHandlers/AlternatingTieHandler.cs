using System;
using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    public interface IAlternatingTieHandler : ITieHandler
    {
        Priority InitialPriority { get; }
        Priority NextPriority { get; }
    }

    public class AlternatingTieHandler : IAlternatingTieHandler
    {
        public Priority InitialPriority { get; private set; }
        public Priority NextPriority { get; private set; }

        public AlternatingTieHandler(Alternated alternated)
        {
            switch (alternated)
            {
                case Alternated.LeftHasPriorityInitially:
                    InitialPriority = Priority.LeftHasPriority;
                    break;
                case Alternated.RightHasPriorityInitially:
                    InitialPriority = Priority.RightHasPriority;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("alternated");
            }
            NextPriority = InitialPriority;
        }

        private AlternatingTieHandler(Priority initialPriority, Priority nextPriority)
        {
            InitialPriority = initialPriority;
            NextPriority = nextPriority;
        }

        public TieResult ResolveTie()
        {
            switch (NextPriority)
            {
                case Priority.LeftHasPriority:
                    NextPriority = Priority.RightHasPriority;
                    return TieResult.Left;
                case Priority.RightHasPriority:
                    NextPriority = Priority.LeftHasPriority;
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ITieHandler DeepCopy()
        {
            return new AlternatingTieHandler(InitialPriority, NextPriority);
        }
    }
}
