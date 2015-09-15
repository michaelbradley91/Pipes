using System;
using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    internal class PrioritisingTieHandler : ITieHandler
    {
        private readonly Priority priority;

        public PrioritisingTieHandler(Priority priority)
        {
            this.priority = priority;
        }

        public TieResult ResolveTie()
        {
            switch (priority)
            {
                case Priority.LeftHasPriority:
                    return TieResult.Left;
                case Priority.RightHasPriority:
                    return TieResult.Right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
