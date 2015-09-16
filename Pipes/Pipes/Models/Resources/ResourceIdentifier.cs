using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Pipes.Models.Resources
{
    internal class ResourceIdentifier
    {
        private ResourceIdentifier parent;
        private readonly BigInteger identifier;
        private readonly Guid guid;
        private readonly Semaphore acquireSemaphore;

        private ResourceIdentifier(BigInteger identifier)
        {
            this.identifier = identifier;
            guid = new Guid();
            parent = this;
            acquireSemaphore = new Semaphore(0, 1);
        }

        public static ResourceIdentifier CreateResourceIdentifierBiggerThan(params ResourceIdentifier[] resourceIdentifiers)
        {
            var maxIdentifier = resourceIdentifiers.Max(r => r.GetRootResourceIdentifier().identifier);
            return new ResourceIdentifier(BigInteger.Add(maxIdentifier, BigInteger.One));
        }

        public ResourceIdentifier GetRootResourceIdentifier()
        {
            if (ReferenceEquals(parent, this)) return this;
            parent = parent.GetRootResourceIdentifier();
            return parent;
        }

        public void Acquire()
        {
            acquireSemaphore.WaitOne();
        }

        public void Free()
        {
            acquireSemaphore.Release();
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
            var comparison = GetRootResourceIdentifier().identifier.CompareTo(other.GetRootResourceIdentifier().identifier);
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
