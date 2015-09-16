using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SharedResources.Helpers;

namespace SharedResources.SharedResources
{
    internal interface ISharedResourceGroup
    {
        ISharedResource CreateAndAcquireSharedResource();
        void ConnectSharedResources(ISharedResource resource1, ISharedResource resource2);
        void FreeSharedResources();
    }

    internal class SharedResourceGroup : ISharedResourceGroup
    {
        private static readonly Gateway SharedResourceAcquisitionGateway = new Gateway();
        private const int NumberOfFailuresAtWhichTheGateShouldBeClosed = 100;

        private readonly IList<SharedResource> sharedResources;
        private bool sharedResourcesAcquired;

        public static SharedResourceGroup CreateWithNoAcquiredSharedResources()
        {
            return new SharedResourceGroup(new SharedResource[] { });
        }

        public static SharedResourceGroup CreateAcquiringSharedResources(params SharedResource[] sharedResources)
        {
            return new SharedResourceGroup(sharedResources);
        }

        private SharedResourceGroup(IEnumerable<SharedResource> sharedResources)
        {
            this.sharedResources = sharedResources.ToList();
            if (this.sharedResources.Any()) AcquireSharedResources();
            sharedResourcesAcquired = true;
        }

        private void AcquireSharedResources()
        {
            var gatePass = SharedResourceAcquisitionGateway.Enter();

            var numberOfFailedAttempts = 0;
            var resourcesToAcquire = sharedResources.ToList();

            while (resourcesToAcquire.Any())
            {
                var resource = GetNextSharedResourceToAcquire(resourcesToAcquire);
                var acquiredResource = TryAcquireSharedResource(resource);

                if (acquiredResource)
                {
                    resourcesToAcquire.RemoveAll(r => r.GetCurrentRootSharedResourceIdentifier().Equals(resource.GetCurrentRootSharedResourceIdentifier()));
                }
                else
                {
                    if (++numberOfFailedAttempts >= NumberOfFailuresAtWhichTheGateShouldBeClosed) SharedResourceAcquisitionGateway.Close(gatePass);
                }
            }
            SharedResourceAcquisitionGateway.Leave(gatePass);
        }

        private static SharedResource GetNextSharedResourceToAcquire(IEnumerable<SharedResource> resources)
        {
            return resources.OrderBy(r => r.GetCurrentRootSharedResourceIdentifier()).First();
        }

        private bool TryAcquireSharedResource(SharedResource sharedResource)
        {
            var resourceIdentifier = sharedResource.GetCurrentRootSharedResourceIdentifier();
            resourceIdentifier.Acquire(this);

            var acquiredRootResourceIdentifier = resourceIdentifier.Equals(sharedResource.GetCurrentRootSharedResourceIdentifier());

            if (!acquiredRootResourceIdentifier) resourceIdentifier.Free(this);
            return acquiredRootResourceIdentifier;
        }

        public ISharedResource CreateAndAcquireSharedResource()
        {
            CheckSharedResourcesAcquired();
            var resourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResources.Select(r => r.GetCurrentRootSharedResourceIdentifier()).ToArray());
            var resource = new SharedResource(resourceIdentifier);
            resource.GetCurrentRootSharedResourceIdentifier().Acquire(this);
            sharedResources.Add(resource);
            return resource;
        }

        public void ConnectSharedResources(ISharedResource resource1, ISharedResource resource2)
        {
            CheckSharedResourcesAcquired();
            CheckSharedResourcesAreInGroup(resource1, resource2);
            if (resource1.Equals(resource2)) return;

            var resource1Identifier = resource1.GetCurrentRootSharedResourceIdentifier();
            var resource2Identifier = resource2.GetCurrentRootSharedResourceIdentifier();
            var parentResourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(resource1Identifier, resource2Identifier);
            parentResourceIdentifier.Acquire(this);

            resource1Identifier.SetParentSharedResourceIdentifier(parentResourceIdentifier);
            resource2Identifier.SetParentSharedResourceIdentifier(parentResourceIdentifier);

            // We must free these SharedResource identifiers to ensure that we do not hold onto them after the group is freed.
            resource1Identifier.Free(this);
            resource2Identifier.Free(this);
        }

        [AssertionMethod]
        private void CheckSharedResourcesAreInGroup(params ISharedResource[] sharedResourcesToCheck)
        {
            if (sharedResourcesToCheck.Any(resource => !sharedResources.Contains(resource)))
            {
                throw new ArgumentException("Cannot manipulate sharedResources not in this SharedResource group");
            }
        }

        public void FreeSharedResources()
        {
            CheckSharedResourcesAcquired();
            sharedResourcesAcquired = false;
            foreach (var resourceIdentifier in sharedResources.Select(r => r.GetCurrentRootSharedResourceIdentifier()).Distinct())
            {
                resourceIdentifier.Free(this);
            }
        }

        private void CheckSharedResourcesAcquired()
        {
            if (!sharedResourcesAcquired)
            {
                throw new InvalidOperationException("You cannot use a SharedResource group after its sharedResources have been freed");
            }
        }
    }
}
