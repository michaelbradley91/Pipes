using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Tests.Helpers
{
    public static class InletHelpers
    {
        public static SimpleInlet<T> CreateInlet<T>(IPipe pipe)
        {
            var promisedPipe = new Promised<IPipe>();
            promisedPipe.Fulfill(pipe);
            return new SimpleInlet<T>(promisedPipe);
        }
    }
}
