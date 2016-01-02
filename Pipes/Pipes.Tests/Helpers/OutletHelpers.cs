using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Tests.Helpers
{
    public static class OutletHelpers
    {
        public static SimpleOutlet<T> CreateOutlet<T>(IPipe pipe)
        {
            var sharedResource = SharedResource.Create();
            return new SimpleOutlet<T>(new Lazy<IPipe>(() => pipe), sharedResource);
        }
    }
}
