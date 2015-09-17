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
            var resource = new SharedResource();

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
            var resource1 = new SharedResource();
            var resource2 = new SharedResource();

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
            var resource1 = new SharedResource();
            var resource2 = new SharedResource();
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
            var resource1 = new SharedResource();
            var resource2 = new SharedResource();
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
            var resource1 = new SharedResource();
            var resource2 = new SharedResource();
            var resourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(resource1.GetCurrentRootSharedResourceIdentifier(), resource2.GetCurrentRootSharedResourceIdentifier());
            resource1.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(resourceIdentifier);
            resource2.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(resourceIdentifier);

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
            var childResource = new SharedResource();
            var parentResource = new SharedResource();
            var child = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(childResource.GetCurrentRootSharedResourceIdentifier(), parentResource.GetCurrentRootSharedResourceIdentifier());
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            childResource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(child);
            parentResource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(parent);
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
            var childResource = new SharedResource();
            var parentResource = new SharedResource();
            var child = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(childResource.GetCurrentRootSharedResourceIdentifier(), parentResource.GetCurrentRootSharedResourceIdentifier());
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            var grandParent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(parent);
            childResource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(child);
            parentResource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(parent);
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
            var resource1 = new SharedResource();
            var resource2 = new SharedResource();

            // Act
            resourceGroup.ConnectSharedResources(resource1, resource2);
        }

        [Test]
        public void ConnectSharedResources_GivenTheSameSharedResourceTwice_DoesNothing()
        {
            // Arrange
            var resource = new SharedResource();
            var resourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource);

            // Act
            resourceGroup.ConnectSharedResources(resource, resource);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectSharedResources_GivenTheSharedResourceGroupHasFreedItsSharedResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resource = new SharedResource();
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

        [Test]
        public void GetSharedResourcesDirectlyConnectedTo_ReturnsTheCorrectSharedResources()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource3 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource4 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            resourceGroup.ConnectSharedResources(sharedResource2, sharedResource3);

            // Act
            var directToSharedResource1 = resourceGroup.GetSharedResourcesDirectlyConnectedTo(sharedResource1);
            var directToSharedResource2 = resourceGroup.GetSharedResourcesDirectlyConnectedTo(sharedResource2);
            var directToSharedResource3 = resourceGroup.GetSharedResourcesDirectlyConnectedTo(sharedResource3);
            var directToSharedResource4 = resourceGroup.GetSharedResourcesDirectlyConnectedTo(sharedResource4);

            // Assert
            directToSharedResource1.Should().HaveCount(2).And.Contain(sharedResource1).And.Contain(sharedResource2);
            directToSharedResource2.Should().HaveCount(3).And.Contain(sharedResource1).And.Contain(sharedResource2).And.Contain(sharedResource3);
            directToSharedResource3.Should().HaveCount(2).And.Contain(sharedResource2).And.Contain(sharedResource3);
            directToSharedResource4.Should().HaveCount(1).And.Contain(sharedResource4);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void DisconnectSharedResources_GivenTheSameSharedResourceTwice_ThrowsAnArgumentException()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource = resourceGroup.CreateAndAcquireSharedResource();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource, sharedResource);
        }

        [Test]
        public void DisconnectSharedResources_GivenTwoPairedSharedResources_RemovesTheDirectConnectionAndProvidesThemWithSeparateSharedResourceIdentifiers()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);

            // Assert
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(sharedResource2.GetCurrentRootSharedResourceIdentifier());
            sharedResource1.DirectlyConnectedSharedResources.Should().NotContain(sharedResource2);
            sharedResource2.DirectlyConnectedSharedResources.Should().NotContain(sharedResource1);
        }

        [Test]
        public void DisconnectSharedResources_GivenTwoDisconnectedSharedResources_DoesNothing()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);

            // Assert
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(sharedResource2.GetCurrentRootSharedResourceIdentifier());
            sharedResource1.DirectlyConnectedSharedResources.Should().NotContain(sharedResource2);
            sharedResource2.DirectlyConnectedSharedResources.Should().NotContain(sharedResource1);
        }

        [Test]
        public void DisconnectSharedResources_GivenTwoPairedSharedResources_FreesTheirOriginalSharedResourceIdentifierAndAcquiresTheNewOnes()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            var originalSharedResourceIdentifier = sharedResource1.GetCurrentRootSharedResourceIdentifier();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);

            // Assert
            originalSharedResourceIdentifier.IsAcquired().Should().BeFalse();
            sharedResource1.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
            sharedResource2.GetCurrentRootSharedResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void DisconnectSharedResources_AfterTheSharedResourcesHaveBeenFreed_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            resourceGroup.FreeSharedResources();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void DisconnectSharedResources_GivenASharedResourceNotInThisResourceGroup_ThrowsAnArgumentException()
        {
            // Arrange
            var tempResourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = tempResourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = tempResourceGroup.CreateAndAcquireSharedResource();
            tempResourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            tempResourceGroup.FreeSharedResources();
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);
        }

        [Test]
        public void DisconnectSharedResources_WhenTheSharedResourcesWillStillBeConnectedIndirectly_LeavesThemWithTheirExistingResourceIdentifierButRemovesTheirDirectConnection()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource3 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            resourceGroup.ConnectSharedResources(sharedResource2, sharedResource3);
            resourceGroup.ConnectSharedResources(sharedResource3, sharedResource1);
            var originalSharedResourceIdentifier1 = sharedResource1.GetCurrentRootSharedResourceIdentifier();
            var originalSharedResourceIdentifier2 = sharedResource2.GetCurrentRootSharedResourceIdentifier();
            var originalSharedResourceIdentifier3 = sharedResource3.GetCurrentRootSharedResourceIdentifier();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource1, sharedResource2);

            // Assert
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().BeSameAs(originalSharedResourceIdentifier1);
            sharedResource2.GetCurrentRootSharedResourceIdentifier().Should().BeSameAs(originalSharedResourceIdentifier2);
            sharedResource3.GetCurrentRootSharedResourceIdentifier().Should().BeSameAs(originalSharedResourceIdentifier3);
        }

        [Test]
        public void DisconnectSharedResources_WhenDisconnectingSplitsConnectedGroups_ReassignsTheSharedResourceIdentifiersToSplitTheDisconnectedGroups()
        {
            // Arrange
            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var sharedResource1 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource2 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource3 = resourceGroup.CreateAndAcquireSharedResource();
            var sharedResource4 = resourceGroup.CreateAndAcquireSharedResource();
            resourceGroup.ConnectSharedResources(sharedResource1, sharedResource2);
            resourceGroup.ConnectSharedResources(sharedResource2, sharedResource3);
            resourceGroup.ConnectSharedResources(sharedResource3, sharedResource4);
            var originalSharedResourceIdentifier = sharedResource1.GetCurrentRootSharedResourceIdentifier();

            // Act
            resourceGroup.DisconnectSharedResources(sharedResource2, sharedResource3);

            // Assert
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().BeSameAs(sharedResource2.GetCurrentRootSharedResourceIdentifier());
            sharedResource2.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(sharedResource3.GetCurrentRootSharedResourceIdentifier());
            sharedResource3.GetCurrentRootSharedResourceIdentifier().Should().BeSameAs(sharedResource4.GetCurrentRootSharedResourceIdentifier());
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(originalSharedResourceIdentifier);
            sharedResource4.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(originalSharedResourceIdentifier);
        }
    }
}
