using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Tests.Helpers
{
    public static class OutletHelpers
    {
        public static SimpleOutlet<T> CreateOutlet<T>(IPipe pipe)
        {
            var promisedPipe = new Promised<IPipe>();
            promisedPipe.Fulfill(pipe);
            return new SimpleOutlet<T>(promisedPipe);
        }
    }
}
