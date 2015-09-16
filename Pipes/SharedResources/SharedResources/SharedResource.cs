namespace SharedResources.SharedResources
{
    internal interface ISharedResource
    {
        SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier();
    }

    internal class SharedResource : ISharedResource
    {
        private readonly SharedResourceIdentifier sharedResourceIdentifier;

        public SharedResource(SharedResourceIdentifier sharedResourceIdentifier)
        {
            this.sharedResourceIdentifier = sharedResourceIdentifier;
        }

        public SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier()
        {
            return sharedResourceIdentifier.GetCurrentRootSharedResourceIdentifier();
        }
    }
}
