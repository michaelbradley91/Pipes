using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class OutletHelpers
    {
        public static SimpleOutlet<T> CreateOutlet<T>(IPipe pipe)
        {
            return new SimpleOutlet<T>(new Lazy<IPipe>(() => pipe));
        }
    }
}
