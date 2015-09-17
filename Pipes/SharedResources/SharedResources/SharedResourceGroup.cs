using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        /// Connects two resources together. A resource can only be acquired if all resources it is connected to are also acquired.
        /// Therefore, use this to indicate a set of resources must all be acquired before any of them can be interacted with.
        /// </summary>
        void ConnectSharedResources(SharedResource resource1, SharedResource resource2);

        /// <summary>
        /// Disconnect the two shared resources
        /// </summary>
        void DisconnectedSharedResources(SharedResource resource1, SharedResource resource2);

        /// <summary>
        /// Disconnects this shared resource from all other shared resources it is connected to
        /// </summary>
        void IsolateSharedResource(SharedResource resource);
        
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
            var resourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResources.Select(r => r.GetCurrentRootSharedResourceIdentifier()).ToArray());
            var resource = new SharedResource(resourceIdentifier);
            resource.GetCurrentRootSharedResourceIdentifier().Acquire(this);
            sharedResources.Add(resource);
            return resource;
        }

        public void ConnectSharedResources(SharedResource resource1, SharedResource resource2)
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

        public void DisconnectedSharedResources(SharedResource resource1, SharedResource resource2)
        {
            throw new NotImplementedException();
        }

        public void IsolateSharedResource(SharedResource resource)
        {
            throw new NotImplementedException();
        }

        [AssertionMethod]
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
