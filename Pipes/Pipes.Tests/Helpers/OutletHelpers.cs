using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class OutletHelpers
    {
        public static Outlet<T> CreateOutlet<T>(IPipe<T> pipe)
        {
            var sharedResource = SharedResourceHelpers.CreateSharedResource();
            return new Outlet<T>(pipe, sharedResource);
        }
    }
}
