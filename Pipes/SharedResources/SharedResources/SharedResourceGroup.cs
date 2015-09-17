using System;
using System.Collections.Generic;
using System.Linq;
using SharedResources.Helpers;

namespace SharedResources.SharedResources
{
    /// <summary>
    /// A shared resource group contains a set of resources which it has acquired - no other thread can acquire these resources
    /// while this group holds onto them.
    /// 
    /// You can have multiple resource groups attempt to acquire the same resources in parallel, but you cannot interact with the same resource *group*
    /// in multiple threads.
    /// </summary>
    public interface ISharedResourceGroup
    {
        /// <summary>
        /// Creates a new shared resource and adds it to this group. The resource is initially acquired by this group, and so
        /// cannot be acquired by other threads.
        /// </summary>
        SharedResource CreateAndAcquireSharedResource();

        /// <summary>
        /// Connects two resources together directly. Note that if the two shared resources are already connected indirectly (through other shared resources)
        /// this will still add the direct connection between the two resources.
        /// A resource can only be acquired if all resources it is connected to are also acquired.
        /// Therefore, use this to indicate a set of resources must all be acquired before any of them can be interacted with.
        /// </summary>
        void ConnectSharedResources(SharedResource resource1, SharedResource resource2);

        /// <summary>
        /// Remove any direct connection between these two resources. Note that if resource1 is also indirectly connected to resource2,
        /// that connection will remain.
        /// </summary>
        void DisconnectSharedResources(SharedResource resource1, SharedResource resource2);

        /// <summary>
        /// Returns a readonly collection of the shared resources directly connected to the given resource.
        /// You can use this in principle to discover the graph making up all your shared resources.
        /// </summary>
        IReadOnlyCollection<SharedResource> GetSharedResourcesDirectlyConnectedTo(SharedResource resource);
        
        /// <summary>
        /// Indicate you are done interacting with the resources in this resource group, and free all resources acquired.
        /// You will not be able to use this resource group after you have called this method.
        /// </summary>
        void FreeSharedResources();
    }

    public class SharedResourceGroup : ISharedResourceGroup
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

        public SharedResource CreateAndAcquireSharedResource()
        {
            CheckSharedResourcesAcquired();
            var resource = new SharedResource();
            resource.GetCurrentRootSharedResourceIdentifier().Acquire(this);
            sharedResources.Add(resource);
            return resource;
        }

        public void ConnectSharedResources(SharedResource resource1, SharedResource resource2)
        {
            CheckSharedResourcesAcquired();
            CheckSharedResourcesAreInGroup(resource1, resource2);
            if (resource1.Equals(resource2) || resource1.DirectlyConnectedSharedResources.Contains(resource2)) return;

            resource1.DirectlyConnect(resource2);

            var resource1Identifier = resource1.GetCurrentRootSharedResourceIdentifier();
            var resource2Identifier = resource2.GetCurrentRootSharedResourceIdentifier();
            if (resource1Identifier.Equals(resource2Identifier)) return;

            var parentResourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(resource1Identifier, resource2Identifier);
            parentResourceIdentifier.Acquire(this);

            resource1Identifier.SetParentSharedResourceIdentifier(parentResourceIdentifier);
            resource2Identifier.SetParentSharedResourceIdentifier(parentResourceIdentifier);

            // We must free these SharedResource identifiers to ensure that we do not hold onto them after the group is freed.
            resource1Identifier.Free(this);
            resource2Identifier.Free(this);
        }

        public void DisconnectSharedResources(SharedResource resource1, SharedResource resource2)
        {
            CheckSharedResourcesAcquired();
            CheckSharedResourcesAreInGroup(resource1, resource2);
            if (resource1 == resource2) throw new ArgumentException("You cannot disconnect a shared resource form itself");
            if (!resource1.DirectlyConnectedSharedResources.Contains(resource2)) return;

            resource1.RemoveDirectConnectionTo(resource2);
            if (resource1.ConnectedSharedResources.Contains(resource2)) return;

            // All resources connected to resource 1 or 2 should have been acquired and have the same root shared resource identifier
            var originalResourceIdentifier = resource1.GetCurrentRootSharedResourceIdentifier();
            var newResourceIdentifierForResource1 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(originalResourceIdentifier);
            newResourceIdentifierForResource1.Acquire(this);
            foreach (var resource in resource1.ConnectedSharedResources)
            {
                resource.ResetRootSharedResourceIdentifier(newResourceIdentifierForResource1);
            }
            var newResourceIdentifierForResource2 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(newResourceIdentifierForResource1);
            newResourceIdentifierForResource2.Acquire(this);
            foreach (var resource in resource2.ConnectedSharedResources)
            {
                resource.ResetRootSharedResourceIdentifier(newResourceIdentifierForResource2);
            }
            originalResourceIdentifier.Free(this);
        }

        public IReadOnlyCollection<SharedResource> GetSharedResourcesDirectlyConnectedTo(SharedResource resource)
        {
            return resource.DirectlyConnectedSharedResources;
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckSharedResourcesAreInGroup(params SharedResource[] sharedResourcesToCheck)
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
