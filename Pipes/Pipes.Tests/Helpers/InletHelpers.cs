using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class InletHelpers
    {
        public static Inlet<T> CreateInlet<T>(IPipe<T> pipe)
        {
            var sharedResource = SharedResourceHelpers.CreateSharedResource();
            return new Inlet<T>(pipe, sharedResource);
        }
    }
}
