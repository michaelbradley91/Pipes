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

        internal SharedResource(SharedResourceIdentifier sharedResourceIdentifier)
        {
            this.sharedResourceIdentifier = sharedResourceIdentifier;
        }

        internal SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier()
        {
            return sharedResourceIdentifier.GetCurrentRootSharedResourceIdentifier();
        }
    }
}
