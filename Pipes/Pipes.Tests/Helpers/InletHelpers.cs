using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Tests.Helpers
{
    public static class InletHelpers
    {
        public static SimpleInlet<T> CreateInlet<T>(IPipe pipe)
        {
            var sharedResource = SharedResource.Create();
            return new SimpleInlet<T>(new Lazy<IPipe>(() => pipe), sharedResource);
        }
    }
}
