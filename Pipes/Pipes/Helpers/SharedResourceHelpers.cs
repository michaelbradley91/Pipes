using SharedResources.SharedResources;

namespace Pipes.Helpers
{
    internal static class SharedResourceHelpers
    {
        public static SharedResource CreateSharedResource()
        {
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var resource = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.FreeSharedResources();
            return resource;
        }
    }
}
