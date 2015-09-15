using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IPrioritisingTieBreaker : ITieBreaker
    {
        Priority Priority { get; }
    }

    public class PrioritisingTieBreaker : IPrioritisingTieBreaker
    {
        public Priority Priority { get; private set; }

        public PrioritisingTieBreaker(Priority priority)
        {
            Priority = priority;
        }

        public TieResult ResolveTie()
        {
            switch (Priority)
            {
                case Priority.Left:
                    return TieResult.Left;
                case Priority.Right:
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ITieBreaker DeepCopy()
        {
            return new PrioritisingTieBreaker(Priority);
        }
    }
}
