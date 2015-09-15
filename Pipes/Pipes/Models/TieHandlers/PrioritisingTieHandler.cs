using System;
using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    public interface IPrioritisingTieHandler : ITieHandler
    {
        Priority Priority { get; }
    }

    public class PrioritisingTieHandler : IPrioritisingTieHandler
    {
        public Priority Priority { get; private set; }

        public PrioritisingTieHandler(Priority priority)
        {
            Priority = priority;
        }

        public TieResult ResolveTie()
        {
            switch (Priority)
            {
                case Priority.LeftHasPriority:
                    return TieResult.Left;
                case Priority.RightHasPriority:
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ITieHandler DeepCopy()
        {
            return new PrioritisingTieHandler(Priority);
        }
    }
}
