using System;
using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    internal class AlternatingTieHandler : ITieHandler
    {
        private Priority nextPriority; 

        public AlternatingTieHandler(Alternated alternated)
        {
            switch (alternated)
            {
                case Alternated.LeftHasPriorityInitially:
                    nextPriority = Priority.LeftHasPriority;
                    break;
                case Alternated.RightHasPriorityInitially:
                    nextPriority = Priority.RightHasPriority;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("alternated");
            }
        }

        public TieResult ResolveTie()
        {
            switch (nextPriority)
            {
                case Priority.LeftHasPriority:
                    nextPriority = Priority.RightHasPriority;
                    return TieResult.Left;
                case Priority.RightHasPriority:
                    nextPriority = Priority.LeftHasPriority;
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
