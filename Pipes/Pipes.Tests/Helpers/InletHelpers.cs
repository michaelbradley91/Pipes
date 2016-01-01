using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class InletHelpers
    {
        public static SimpleInlet<T> CreateInlet<T>(IPipe pipe)
        {
            var sharedResource = SharedResourceHelpers.CreateSharedResource();
            return new SimpleInlet<T>(new Lazy<IPipe>(() => pipe), sharedResource);
        }
    }
}
