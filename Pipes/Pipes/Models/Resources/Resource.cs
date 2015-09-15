namespace Pipes.Models.Resources
{
    internal interface IResource
    {
        ResourceIdentifier GetCurrentIdentifier();
        void Acquire();
        void Free();
    }

    internal class Resource : IResource
    {
        private readonly ResourceIdentifier resourceIdentifier;

        public Resource(ResourceIdentifier resourceIdentifier)
        {
            this.resourceIdentifier = resourceIdentifier;
        }

        public ResourceIdentifier GetCurrentIdentifier()
        {
            return resourceIdentifier;
        }

        public void Acquire()
        {
            resourceIdentifier.Acquire();
        }

        public void Free()
        {
            resourceIdentifier.Free();
        }
    }
}
