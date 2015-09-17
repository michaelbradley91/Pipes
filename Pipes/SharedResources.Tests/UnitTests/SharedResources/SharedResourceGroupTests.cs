using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SharedResources.SharedResources;

namespace SharedResources.Tests.UnitTests.SharedResources
{
    [TestFixture]
    public class ResourceGroupTests
    {
        [Test]
        public void CreateNew_ReturnsASharedResourceGroup()
        {
            // Act
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();

            // Assert
            resourceGroup.Should().NotBeNull();
        }

        [Test]
        public void CreateAndAcquireSharedResource_ReturnsTheSharedResourceAndHasAcquiredIt()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();

            // Act
            var resource = resourceGroup.CreateAndAcquireSharedResource();

            // Assert (we can check the resource has been acquired by trying to free it with the group)
            resource.Should().NotBeNull();
            resource.GetCurrentRootSharedResourceIdentifier().Should().NotBeNull();
            resource.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAndAcquireSharedResource_WhenCalledMultipleTimes_ReturnsDifferentResources()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();

            // Act
            var resource1 = resourceGroup.CreateAndAcquireSharedResource();
            var resource2 = resourceGroup.CreateAndAcquireSharedResource();

            // Assert
            resource1.Should().NotBeSameAs(resource2);
        }

        [Test]
        public void CreateAndAcquireSharedResource_WhenCalledMultipleTimes_ReturnsSharedResourcesWithDistinctSharedResourceIdentifiers()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var resource1 = resourceGroup.CreateAndAcquireSharedResource();
            var resource2 = resourceGroup.CreateAndAcquireSharedResource();

            // Act
            var resourceIdentifier1 = resource1.GetCurrentRootSharedResourceIdentifier();
            var resourceIdentifier2 = resource2.GetCurrentRootSharedResourceIdentifier();

            // Assert
            resourceIdentifier1.Should().NotBeSameAs(resourceIdentifier2);
        }

        [Test]
        public void ConnectSharedResources_GivenTwoSharedResources_EnsuresTheyHaveTheSameSharedResourceIdentifierAndAcquiresIt()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var resource1 = resourceGroup.CreateAndAcquireSharedResource();
            var resource2 = resourceGroup.CreateAndAcquireSharedResource();

            // Act
            resourceGroup.ConnectSharedResources(resource1, resource2);
            var resourceIdentifier1 = resource1.GetCurrentRootSharedResourceIdentifier();
            var resourceIdentifier2 = resource2.GetCurrentRootSharedResourceIdentifier();

            // Assert
            resourceIdentifier1.Should().BeSameAs(resourceIdentifier2);
            resourceIdentifier1.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void ConnectSharedResources_GivenTwoSharedResources_FreesTheirPreviousSharedResourceIdentifiersAsItNoLongerNeedsThem()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var resource1 = resourceGroup.CreateAndAcquireSharedResource();
            var resource2 = resourceGroup.CreateAndAcquireSharedResource();
            var previousResourceIdentifier1 = resource1.GetCurrentRootSharedResourceIdentifier();
            var previousResourceIdentifier2 = resource2.GetCurrentRootSharedResourceIdentifier();

            // Act
            resourceGroup.ConnectSharedResources(resource1, resource2);

            // Assert
            previousResourceIdentifier1.IsAcquired().Should().BeFalse();
            previousResourceIdentifier2.IsAcquired().Should().BeFalse();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenASingleSharedResource_AcquiresThatSharedResource()
        {
            // Arrange
            var resource = new SharedResource(SharedResourceIdentifier.Create());

            // Act
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource);

            // Assert
            resource.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenNoSharedResources_ReturnsASharedResourceGroup()
        {
            // Act
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources();

            // Assert
            resourceGroup.Should().NotBeNull();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenMultipleSharedResourcesWithNoCompetition_AcquiresThemAll()
        {
            // Arrange
            var resource1 = new SharedResource(SharedResourceIdentifier.Create());
            var resource2 = new SharedResource(SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan());

            // Act
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource1, resource2);

            // Assert
            resource1.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
            resource2.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void FreeSharedResources_GivenTheSharedResourceGroupHasNoSharedResources_DoesNothing()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();

            // Act
            resourceGroup.FreeSharedResources();
        }

        [Test]
        public void FreeSharedResources_GivenTheSharedResourceGroupHasMultipleSharedResources_FreesThemAll()
        {
            // Arrange
            var resource1 = new SharedResource(SharedResourceIdentifier.Create());
            var resource2 = new SharedResource(SharedResourceIdentifier.Create());
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource1, resource2);

            // Act
            resourceGroup.FreeSharedResources();
            
            // Assert
            resource1.GetCurrentRootSharedResourceIdentifier().IsAcquired().Should().BeFalse();
            resource2.GetCurrentRootSharedResourceIdentifier().IsAcquired().Should().BeFalse();
        }

        [Test]
        public void FreeSharedResources_GivenTheSharedResourceGroupHasSharedResourcesThatHaveBeenConnected_FreesThemAll()
        {
            // Arrange
            var resource1 = new SharedResource(SharedResourceIdentifier.Create());
            var resource2 = new SharedResource(SharedResourceIdentifier.Create());
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource1, resource2);
            resourceGroup.ConnectSharedResources(resource1, resource2);

            // Act
            resourceGroup.FreeSharedResources();

            // Assert
            resource1.GetCurrentRootSharedResourceIdentifier().IsAcquired().Should().BeFalse();
            resource2.GetCurrentRootSharedResourceIdentifier().IsAcquired().Should().BeFalse();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenMultipleSharedResourcesWithTheSameSharedResourceIdentifier_CanAcquireThemAll()
        {
            // Arrange
            var resourceIdentifier = SharedResourceIdentifier.Create();
            var resource1 = new SharedResource(resourceIdentifier);
            var resource2 = new SharedResource(resourceIdentifier);

            // Act
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource1, resource2);

            // Assert
            resource1.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
            resource2.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenMultipleSharedResources_AcquiresTheSharedResourcesInOrderOfSharedResourceIdentifierAscending()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            var childResource = new SharedResource(child);
            var parentResource = new SharedResource(parent);
            var competingResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(childResource, parentResource);

            // Act
            SharedResourceGroup resourceGroup = null;
            var thread = new Thread(() => resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(childResource, parentResource));
            thread.Start();
            Thread.Sleep(500);

            // Assert
            child.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            parent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();

            // Act
            parent.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            child.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            parent.IsAcquired().Should().BeFalse();

            // Act
            parent.Acquire(competingResourceGroup);
            child.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            child.IsAcquired().Should().BeTrue();
            child.IsAcquiredBy(competingResourceGroup).Should().BeFalse();
            parent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();

            // Act
            parent.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            resourceGroup.Should().NotBeNull();
            child.IsAcquiredBy(resourceGroup).Should().BeTrue();
            parent.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAcquiringSharedResources_GivenTheSharedResourceIdentifierChangesAndThatChangesTheOrderInWhichToAcquireSharedResources_CanStillAcquireSharedResourcesEventually()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            var grandParent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(parent);
            var childResource = new SharedResource(child);
            var parentResource = new SharedResource(parent);
            var competingResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(childResource, parentResource);

            // Act
            SharedResourceGroup resourceGroup = null;
            var thread = new Thread(() => resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(childResource, parentResource));
            thread.Start();
            Thread.Sleep(500);

            // Assert
            child.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            parent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();

            // Act
            child.SetParentSharedResourceIdentifier(grandParent);
            child.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            child.IsAcquired().Should().BeFalse();
            parent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            grandParent.IsAcquired().Should().BeFalse();

            // Act
            grandParent.Acquire(competingResourceGroup);
            parent.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            grandParent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            parent.IsAcquired().Should().BeTrue();

            // Act
            grandParent.Free(competingResourceGroup);
            Thread.Sleep(500);

            // Assert
            resourceGroup.Should().NotBeNull();
            child.IsAcquired().Should().BeFalse();
            parent.IsAcquiredBy(resourceGroup).Should().BeTrue();
            grandParent.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void ConnectSharedResources_WhenGivenASharedResourceNotInTheSharedResourceGroup_ThrowsAnArgumentException()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var resource1 = new SharedResource(SharedResourceIdentifier.Create());
            var resource2 = new SharedResource(SharedResourceIdentifier.Create());

            // Act
            resourceGroup.ConnectSharedResources(resource1, resource2);
        }

        [Test]
        public void ConnectSharedResources_GivenTheSameSharedResourceTwice_DoesNothing()
        {
            // Arrange
            var resource = new SharedResource(SharedResourceIdentifier.Create());
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource);

            // Act
            resourceGroup.ConnectSharedResources(resource, resource);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectSharedResources_GivenTheSharedResourceGroupHasFreedItsSharedResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resource = new SharedResource(SharedResourceIdentifier.Create());
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource);
            resourceGroup.FreeSharedResources();

            // Act
            resourceGroup.ConnectSharedResources(resource, resource);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void CreateAndAcquireSharedResource_GivenTheSharedResourceGroupHasFreedItsSharedResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            resourceGroup.FreeSharedResources();

            // Act
            resourceGroup.CreateAndAcquireSharedResource();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FreeSharedResources_GivenTheSharedResourceGroupHasFreedItsSharedResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            resourceGroup.FreeSharedResources();

            // Act
            resourceGroup.FreeSharedResources();
        }
    }
}
