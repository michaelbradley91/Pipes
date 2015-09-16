using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SharedResources.SharedResources
{
    internal class SharedResourceIdentifier : IComparable<SharedResourceIdentifier>
    {
        private SharedResourceIdentifier parent;
        private readonly BigInteger identifier;
        private readonly Guid guid;
        private readonly Semaphore acquireSemaphore;
        private ISharedResourceGroup acquirer;

        private SharedResourceIdentifier(BigInteger identifier)
        {
            this.identifier = identifier;
            guid = Guid.NewGuid();
            parent = this;
            acquireSemaphore = new Semaphore(1, 1);
        }

        public static SharedResourceIdentifier Create()
        {
            return CreateSharedResourceIdentifierBiggerThan();
        }

        public static SharedResourceIdentifier CreateSharedResourceIdentifierBiggerThan(params SharedResourceIdentifier[] sharedResourceIdentifiers)
        {
            if (!sharedResourceIdentifiers.Any()) return new SharedResourceIdentifier(BigInteger.Zero);
            var maxIdentifier = sharedResourceIdentifiers.Max(r => r.GetCurrentRootSharedResourceIdentifier().identifier);
            return new SharedResourceIdentifier(BigInteger.Add(maxIdentifier, BigInteger.One));
        }

        public SharedResourceIdentifier GetCurrentRootSharedResourceIdentifier()
        {
            if (ReferenceEquals(parent, this)) return this;
            parent = parent.GetCurrentRootSharedResourceIdentifier();
            return parent;
        }

        public void Acquire(ISharedResourceGroup sharedResourceGroup)
        {
            acquireSemaphore.WaitOne();
            acquirer = sharedResourceGroup;
        }

        public void Free(ISharedResourceGroup sharedResourceGroup)
        {
            if (acquirer != sharedResourceGroup) throw new ArgumentException("The resource group given has not acquired this resource and so cannot free it.", "sharedResourceGroup");
            acquirer = null;
            acquireSemaphore.Release();
        }

        public bool IsAcquired()
        {
            return acquirer != null;
        }

        public bool IsAcquiredBy(ISharedResourceGroup sharedResourceGroup)
        {
            return acquirer == sharedResourceGroup;
        }

        public void SetParentSharedResourceIdentifier(SharedResourceIdentifier parentSharedResourceIdentifier)
        {
            if (!ReferenceEquals(parentSharedResourceIdentifier.parent, parentSharedResourceIdentifier))
            {
                throw new ArgumentException("The parent resource identifier cannot also have a parent resource identifier", "parentSharedResourceIdentifier");
            }
            if (parentSharedResourceIdentifier.CompareTo(this) < 0)
            {
                throw new ArgumentException("The parent resource identifier must be at least as large as this resource identifier", "parentSharedResourceIdentifier");
            }
            parent = parentSharedResourceIdentifier;
        }

        public int CompareTo(SharedResourceIdentifier other)
        {
            if (ReferenceEquals(other, this)) return 0;
            var comparison = GetCurrentRootSharedResourceIdentifier().identifier.CompareTo(other.GetCurrentRootSharedResourceIdentifier().identifier);
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
