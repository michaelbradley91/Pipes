using System.Collections.Generic;
using System.Linq;
using Pipes.Helpers;

namespace Pipes.Models.Resources
{
    internal interface IResourceGroup
    {
    }

    internal class ResourceGroup : IResourceGroup
    {
        private static readonly Gateway ResourceAcquisitionGateway = new Gateway();
        private const int NumberOfFailuresAtWhichTheGateShouldBeClosed = 100;

        private readonly IList<Resource> resources;

        private ResourceGroup(IEnumerable<Resource> resources)
        {
            this.resources = resources.ToList();
        }

        public static ResourceGroup CreateNew()
        {
            return new ResourceGroup(new Resource[] { });
        }

        public static ResourceGroup AcquireResources(IReadOnlyCollection<Resource> resources)
        {
            var gatePass = ResourceAcquisitionGateway.Enter();

            var numberOfFailedAttempts = 0;
            var resourcesToAcquire = resources.ToList();

            while (resourcesToAcquire.Any())
            {
                var resource = GetNextResourceToAcquire(resourcesToAcquire);
                var acquiredResource = TryAcquireResource(resource);

                if (acquiredResource)
                {
                    resourcesToAcquire.Remove(resource);
                }
                else
                {
                    if (++numberOfFailedAttempts >= NumberOfFailuresAtWhichTheGateShouldBeClosed) ResourceAcquisitionGateway.Close(gatePass);
                }
            }

            ResourceAcquisitionGateway.Leave(gatePass);
            return new ResourceGroup(resources);
        }

        private static Resource GetNextResourceToAcquire(IEnumerable<Resource> resources)
        {
            return resources.OrderBy(r => r.GetCurrentRootResourceIdentifier()).First();
        }

        private static bool TryAcquireResource(Resource resource)
        {
            var resourceIdentifier = resource.GetCurrentRootResourceIdentifier();
            resourceIdentifier.Acquire();

            var acquiredRootResourceIdentifier = resourceIdentifier.Equals(resource.GetCurrentRootResourceIdentifier());

            if (!acquiredRootResourceIdentifier) resourceIdentifier.Free();
            return acquiredRootResourceIdentifier;
        }
    }
}
