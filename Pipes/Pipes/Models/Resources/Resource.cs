namespace Pipes.Models.Resources
{
    internal interface IResource
    {
        ResourceIdentifier GetCurrentRootResourceIdentifier();
    }

    internal class Resource : IResource
    {
        private readonly ResourceIdentifier resourceIdentifier;

        public Resource(ResourceIdentifier resourceIdentifier)
        {
            this.resourceIdentifier = resourceIdentifier;
        }

        public ResourceIdentifier GetCurrentRootResourceIdentifier()
        {
            return resourceIdentifier.GetCurrentRootResourceIdentifier();
        }
    }
}
