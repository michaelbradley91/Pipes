using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedResources.SharedResources
{
    /// <summary>
    /// The symbolic representation of a shared resource. This can be acquired and created by resource groups.
    /// 
    /// A shared resource is similar to a semaphore / lock in that you can acquire and free it. However, resource groups can connect
    /// shared resources to enforce that acquiring one resource acquires all resources it is connected to.
    /// </summary>
    public class SharedResource
    {
        private readonly SharedResourceIdentifier sharedResourceIdentifier;
        private readonly List<SharedResource> directlyConnectedSharedSharedResources;

        private IReadOnlyCollection<SharedResource> connectedSharedResourcesCache;
        private bool mustRecalculateConnectedSharedResources;

        /// <summary>
        /// Assign this to any object you wish to associate with this resource. This can be used to "remember" what acquiring this shared resource actually acquires.
        /// </summary>
        public object AssociatedObject { get; set; }

        internal SharedResource()
        {
            sharedResourceIdentifier = SharedResourceIdentifier.Create();
            directlyConnectedSharedSharedResources = new List<SharedResource> { this };
            mustRecalculateConnectedSharedResources = true;
            connectedSharedResourcesCache = GetConnectedSharedResources();
        }

        /// <summary>
        /// Sets the shared resource identifier to be used as the new root. This will cut any existing chain to the root shared resource identifier
        /// </summary>
        internal void ResetRootSharedResourceIdentifier(SharedResourceIdentifier newRootSharedResourceIdentifier)
        {
            sharedResourceIdentifier.SetParentSharedResourceIdentifier(newRootSharedResourceIdentifier);
        }

        internal SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier()
        {
            return sharedResourceIdentifier.GetCurrentRootSharedResourceIdentifier();
        }

        internal void DirectlyConnect(SharedResource sharedResource)
        {
            if (!directlyConnectedSharedSharedResources.Contains(sharedResource))
            {
                directlyConnectedSharedSharedResources.Add(sharedResource);
                mustRecalculateConnectedSharedResources = sharedResource.mustRecalculateConnectedSharedResources = true;
            }
            if (!sharedResource.directlyConnectedSharedSharedResources.Contains(this))
            {
                sharedResource.directlyConnectedSharedSharedResources.Add(this);
                mustRecalculateConnectedSharedResources = sharedResource.mustRecalculateConnectedSharedResources = true;
            }
        }

        internal void RemoveDirectConnectionTo(SharedResource sharedResource)
        {
            if (sharedResource == this) throw new ArgumentException("You cannot disconnect a resource from itself.", "sharedResource");
            if (directlyConnectedSharedSharedResources.Contains(sharedResource))
            {
                directlyConnectedSharedSharedResources.Remove(sharedResource);
                mustRecalculateConnectedSharedResources = sharedResource.mustRecalculateConnectedSharedResources = true;
            }
            if (sharedResource.directlyConnectedSharedSharedResources.Contains(this))
            {
                sharedResource.directlyConnectedSharedSharedResources.Remove(this);
                mustRecalculateConnectedSharedResources = sharedResource.mustRecalculateConnectedSharedResources = true;
            }
        }

        internal IReadOnlyCollection<SharedResource> DirectlyConnectedSharedResources
        {
            get { return directlyConnectedSharedSharedResources; }
        }

        internal IReadOnlyCollection<SharedResource> ConnectedSharedResources
        {
            get
            {
                if (mustRecalculateConnectedSharedResources) connectedSharedResourcesCache = GetConnectedSharedResources();
                mustRecalculateConnectedSharedResources = false;
                return connectedSharedResourcesCache;
            }
        }

        private IReadOnlyCollection<SharedResource> GetConnectedSharedResources()
        {
            var allConnectedSharedResources = new HashSet<SharedResource>();
            var sharedResourcesToCheck = new Stack<SharedResource>();
            sharedResourcesToCheck.Push(this);
            while (sharedResourcesToCheck.Any())
            {
                var nextSharedResourceToCheck = sharedResourcesToCheck.Pop();
                foreach (var directlyConnectedResource in nextSharedResourceToCheck.DirectlyConnectedSharedResources)
                {
                    if (allConnectedSharedResources.Contains(directlyConnectedResource)) continue;

                    allConnectedSharedResources.Add(directlyConnectedResource);
                    sharedResourcesToCheck.Push(directlyConnectedResource);
                }
            }
            return allConnectedSharedResources.ToList();
        }
    }
}
