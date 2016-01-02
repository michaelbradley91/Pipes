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

        public static SharedResource CreateAndConnectSharedResource(SharedResource parentResource)
        {
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(parentResource);

            var childResource = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(parentResource, childResource);
            resourceGroup.FreeSharedResources();

            return childResource;
        }

        public static SharedResource CreateAndConnectSharedResources(params SharedResource[] childResources)
        {
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(childResources);

            var parentResource = resourceGroup.CreateAndAcquireSharedResource();
            foreach (var childResource in childResources)
            {
                resourceGroup.ConnectSharedResources(parentResource, childResource);
            }
            resourceGroup.FreeSharedResources();

            return parentResource;
        }
    }
}
