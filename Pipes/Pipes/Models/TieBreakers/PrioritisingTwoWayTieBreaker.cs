using System;
using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface IPrioritisingTwoWayTieBreaker : ITwoWayTieBreaker
    {
        Priority Priority { get; }
    }

    public class PrioritisingTwoWayTieBreaker : TwoWayTieBreaker, IPrioritisingTwoWayTieBreaker
    {
        public Priority Priority { get; }

        public PrioritisingTwoWayTieBreaker(Priority priority)
        {
            Priority = priority;
        }

        public override TieResult ResolveTie()
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
    }
}
