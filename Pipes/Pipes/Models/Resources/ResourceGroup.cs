using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pipes.Helpers;

namespace Pipes.Models.Resources
{
    internal interface IResourceGroup
    {
        IResource CreateAndAcquireResource();
        void ConnectResources(IResource resource1, IResource resource2);
        void FreeResources();
    }

    internal class ResourceGroup : IResourceGroup
    {
        private static readonly Gateway ResourceAcquisitionGateway = new Gateway();
        private const int NumberOfFailuresAtWhichTheGateShouldBeClosed = 100;

        private readonly IList<Resource> resources;
        private bool resourcesAcquired;

        public static ResourceGroup CreateWithNoAcquiredResources()
        {
            return new ResourceGroup(new Resource[] { });
        }

        public static ResourceGroup CreateAcquiringResources(params Resource[] resources)
        {
            return new ResourceGroup(resources);
        }

        private ResourceGroup(IEnumerable<Resource> resources)
        {
            this.resources = resources.ToList();
            if (this.resources.Any()) AcquireResources();
            resourcesAcquired = true;
        }

        private void AcquireResources()
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
                    resourcesToAcquire.RemoveAll(r => r.GetCurrentRootResourceIdentifier().Equals(resource.GetCurrentRootResourceIdentifier()));
                }
                else
                {
                    if (++numberOfFailedAttempts >= NumberOfFailuresAtWhichTheGateShouldBeClosed) ResourceAcquisitionGateway.Close(gatePass);
                }
            }
            ResourceAcquisitionGateway.Leave(gatePass);
        }

        private static Resource GetNextResourceToAcquire(IEnumerable<Resource> resources)
        {
            return resources.OrderBy(r => r.GetCurrentRootResourceIdentifier()).First();
        }

        private bool TryAcquireResource(Resource resource)
        {
            var resourceIdentifier = resource.GetCurrentRootResourceIdentifier();
            resourceIdentifier.Acquire(this);

            var acquiredRootResourceIdentifier = resourceIdentifier.Equals(resource.GetCurrentRootResourceIdentifier());

            if (!acquiredRootResourceIdentifier) resourceIdentifier.Free(this);
            return acquiredRootResourceIdentifier;
        }

        public IResource CreateAndAcquireResource()
        {
            CheckResourcesAcquired();
            var resourceIdentifier = ResourceIdentifier.CreateResourceIdentifierBiggerThan(resources.Select(r => r.GetCurrentRootResourceIdentifier()).ToArray());
            var resource = new Resource(resourceIdentifier);
            resource.GetCurrentRootResourceIdentifier().Acquire(this);
            resources.Add(resource);
            return resource;
        }

        public void ConnectResources(IResource resource1, IResource resource2)
        {
            CheckResourcesAcquired();
            CheckResourcesAreInGroup(resource1, resource2);

            var resource1Identifier = resource1.GetCurrentRootResourceIdentifier();
            var resource2Identifier = resource2.GetCurrentRootResourceIdentifier();
            var parentResourceIdentifier = ResourceIdentifier.CreateResourceIdentifierBiggerThan(resource1Identifier, resource2Identifier);
            parentResourceIdentifier.Acquire(this);

            resource1Identifier.SetParentResourceIdentifier(parentResourceIdentifier);
            resource2Identifier.SetParentResourceIdentifier(parentResourceIdentifier);

            // We must free these resource identifiers to ensure that we do not hold onto them after the group is freed.
            resource1Identifier.Free(this);
            resource2Identifier.Free(this);
        }

        [AssertionMethod]
        private static void CheckResourcesAreInGroup(params IResource[] resourcesToCheck)
        {
            if (resourcesToCheck.Any(resource => !resourcesToCheck.Contains(resource)))
            {
                throw new ArgumentException("Cannot manipulate resources not in this resource group");
            }
        }

        public void FreeResources()
        {
            resourcesAcquired = false;
            foreach (var resourceIdentifier in resources.Select(r => r.GetCurrentRootResourceIdentifier()))
            {
                resourceIdentifier.Free(this);
            }
        }

        private void CheckResourcesAcquired()
        {
            if (!resourcesAcquired) throw new InvalidOperationException("You cannot use a resource group after its resources have been freed");
        }
    }
}
