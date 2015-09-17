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

        internal SharedResource(SharedResourceIdentifier sharedResourceIdentifier)
        {
            this.sharedResourceIdentifier = sharedResourceIdentifier;
            directlyConnectedSharedSharedResources = new List<SharedResource> { this };
        }

        internal SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier()
        {
            return sharedResourceIdentifier.GetCurrentRootSharedResourceIdentifier();
        }

        internal void DirectlyConnect(SharedResource sharedResource)
        {
            if (!directlyConnectedSharedSharedResources.Contains(sharedResource)) directlyConnectedSharedSharedResources.Add(sharedResource);
            if (!sharedResource.directlyConnectedSharedSharedResources.Contains(this)) sharedResource.directlyConnectedSharedSharedResources.Add(this);
        }

        internal void RemoveDirectConnectionTo(SharedResource sharedResource)
        {
            if (sharedResource == this) throw new ArgumentException("You cannot disconnect a resource from itself.", "sharedResource");
            if (directlyConnectedSharedSharedResources.Contains(sharedResource)) directlyConnectedSharedSharedResources.Remove(sharedResource);
            if (sharedResource.directlyConnectedSharedSharedResources.Contains(this)) sharedResource.directlyConnectedSharedSharedResources.Remove(this);
        }

        internal IReadOnlyCollection<SharedResource> DirectlyConnectedSharedResources
        {
            get { return directlyConnectedSharedSharedResources; }
        }

        internal IReadOnlyCollection<SharedResource> ConnectedSharedResources
        {
            get
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
}
