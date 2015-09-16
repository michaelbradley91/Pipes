using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Pipes.Models.Resources
{
    internal class ResourceIdentifier : IComparable<ResourceIdentifier>
    {
        private ResourceIdentifier parent;
        private readonly BigInteger identifier;
        private readonly Guid guid;
        private readonly Semaphore acquireSemaphore;
        private IResourceGroup acquirer;

        private ResourceIdentifier(BigInteger identifier)
        {
            this.identifier = identifier;
            guid = Guid.NewGuid();
            parent = this;
            acquireSemaphore = new Semaphore(1, 1);
        }

        public static ResourceIdentifier Create()
        {
            return CreateResourceIdentifierBiggerThan();
        }

        public static ResourceIdentifier CreateResourceIdentifierBiggerThan(params ResourceIdentifier[] resourceIdentifiers)
        {
            if (!resourceIdentifiers.Any()) return new ResourceIdentifier(BigInteger.Zero);
            var maxIdentifier = resourceIdentifiers.Max(r => r.GetCurrentRootResourceIdentifier().identifier);
            return new ResourceIdentifier(BigInteger.Add(maxIdentifier, BigInteger.One));
        }

        public ResourceIdentifier GetCurrentRootResourceIdentifier()
        {
            if (ReferenceEquals(parent, this)) return this;
            parent = parent.GetCurrentRootResourceIdentifier();
            return parent;
        }

        public void Acquire(IResourceGroup resourceGroup)
        {
            acquireSemaphore.WaitOne();
            acquirer = resourceGroup;
        }

        public void Free(IResourceGroup resourceGroup)
        {
            if (acquirer != resourceGroup) throw new ArgumentException("The resource group given has not acquired this resource and so cannot free it.", "resourceGroup");
            acquirer = null;
            acquireSemaphore.Release();
        }

        public bool IsAcquired()
        {
            return acquirer != null;
        }

        public bool IsAcquiredBy(IResourceGroup resourceGroup)
        {
            return acquirer == resourceGroup;
        }

        public void SetParentResourceIdentifier(ResourceIdentifier parentResourceIdentifier)
        {
            if (!ReferenceEquals(parentResourceIdentifier.parent, parentResourceIdentifier))
            {
                throw new ArgumentException("The parent resource identifier cannot also have a parent resource identifier", "parentResourceIdentifier");
            }
            if (parentResourceIdentifier.CompareTo(this) < 0)
            {
                throw new ArgumentException("The parent resource identifier must be at least as large as this resource identifier", "parentResourceIdentifier");
            }
            parent = parentResourceIdentifier;
        }

        public int CompareTo(ResourceIdentifier other)
        {
            if (ReferenceEquals(other, this)) return 0;
            var comparison = GetCurrentRootResourceIdentifier().identifier.CompareTo(other.GetCurrentRootResourceIdentifier().identifier);
            return comparison != 0 ? comparison : guid.CompareTo(other.guid);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }
}
