using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Resources;

namespace Pipes.Tests.UnitTests.Models.Resources
{
    [TestFixture]
    public class ResourceGroupTests
    {
        [Test]
        public void CreateNew_ReturnsAResourceGroup()
        {
            // Act
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();

            // Assert
            resourceGroup.Should().NotBeNull();
        }

        [Test]
        public void CreateAndAcquireResource_ReturnsTheResourceAndHasAcquiredIt()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();

            // Act
            var resource = resourceGroup.CreateAndAcquireResource();

            // Assert (we can check the resource has been acquired by trying to free it with the group)
            resource.Should().NotBeNull();
            resource.GetCurrentRootResourceIdentifier().Should().NotBeNull();
            resource.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAndAcquireResource_WhenCalledMultipleTimes_ReturnsDifferentResources()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();

            // Act
            var resource1 = resourceGroup.CreateAndAcquireResource();
            var resource2 = resourceGroup.CreateAndAcquireResource();

            // Assert
            resource1.Should().NotBeSameAs(resource2);
        }

        [Test]
        public void CreateAndAcquireResource_WhenCalledMultipleTimes_ReturnsResourcesWithDistinctResourceIdentifiers()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            var resource1 = resourceGroup.CreateAndAcquireResource();
            var resource2 = resourceGroup.CreateAndAcquireResource();

            // Act
            var resourceIdentifier1 = resource1.GetCurrentRootResourceIdentifier();
            var resourceIdentifier2 = resource2.GetCurrentRootResourceIdentifier();

            // Assert
            resourceIdentifier1.Should().NotBeSameAs(resourceIdentifier2);
        }

        [Test]
        public void ConnectResources_GivenTwoResources_EnsuresTheyHaveTheSameResourceIdentifierAndAcquiresIt()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            var resource1 = resourceGroup.CreateAndAcquireResource();
            var resource2 = resourceGroup.CreateAndAcquireResource();

            // Act
            resourceGroup.ConnectResources(resource1, resource2);
            var resourceIdentifier1 = resource1.GetCurrentRootResourceIdentifier();
            var resourceIdentifier2 = resource2.GetCurrentRootResourceIdentifier();

            // Assert
            resourceIdentifier1.Should().BeSameAs(resourceIdentifier2);
            resourceIdentifier1.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void ConnectResources_GivenTwoResources_FreesTheirPreviousResourceIdentifiersAsItNoLongerNeedsThem()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            var resource1 = resourceGroup.CreateAndAcquireResource();
            var resource2 = resourceGroup.CreateAndAcquireResource();
            var previousResourceIdentifier1 = resource1.GetCurrentRootResourceIdentifier();
            var previousResourceIdentifier2 = resource2.GetCurrentRootResourceIdentifier();

            // Act
            resourceGroup.ConnectResources(resource1, resource2);

            // Assert
            previousResourceIdentifier1.IsAcquired().Should().BeFalse();
            previousResourceIdentifier2.IsAcquired().Should().BeFalse();
        }

        [Test]
        public void CreateAcquiringResources_GivenASingleResource_AcquiresThatResource()
        {
            // Arrange
            var resource = new Resource(ResourceIdentifier.Create());

            // Act
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource);

            // Assert
            resource.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAcquiringResources_GivenNoResources_ReturnsAResourceGroup()
        {
            // Act
            var resourceGroup = ResourceGroup.CreateAcquiringResources();

            // Assert
            resourceGroup.Should().NotBeNull();
        }

        [Test]
        public void CreateAcquiringResources_GivenMultipleResourcesWithNoCompetition_AcquiresThemAll()
        {
            // Arrange
            var resource1 = new Resource(ResourceIdentifier.Create());
            var resource2 = new Resource(ResourceIdentifier.CreateResourceIdentifierBiggerThan());

            // Act
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource1, resource2);

            // Assert
            resource1.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
            resource2.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void FreeResources_GivenTheResourceGroupHasNoResources_DoesNothing()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();

            // Act
            resourceGroup.FreeResources();
        }

        [Test]
        public void FreeResources_GivenTheResourceGroupHasMultipleResources_FreesThemAll()
        {
            // Arrange
            var resource1 = new Resource(ResourceIdentifier.Create());
            var resource2 = new Resource(ResourceIdentifier.Create());
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource1, resource2);

            // Act
            resourceGroup.FreeResources();
            
            // Assert
            resource1.GetCurrentRootResourceIdentifier().IsAcquired().Should().BeFalse();
            resource2.GetCurrentRootResourceIdentifier().IsAcquired().Should().BeFalse();
        }

        [Test]
        public void FreeResources_GivenTheResourceGroupHasResourcesThatHaveBeenConnected_FreesThemAll()
        {
            // Arrange
            var resource1 = new Resource(ResourceIdentifier.Create());
            var resource2 = new Resource(ResourceIdentifier.Create());
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource1, resource2);
            resourceGroup.ConnectResources(resource1, resource2);

            // Act
            resourceGroup.FreeResources();

            // Assert
            resource1.GetCurrentRootResourceIdentifier().IsAcquired().Should().BeFalse();
            resource2.GetCurrentRootResourceIdentifier().IsAcquired().Should().BeFalse();
        }

        [Test]
        public void CreateAcquiringResources_GivenMultipleResourcesWithTheSameResourceIdentifier_CanAcquireThemAll()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resource1 = new Resource(resourceIdentifier);
            var resource2 = new Resource(resourceIdentifier);

            // Act
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource1, resource2);

            // Assert
            resource1.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
            resource2.GetCurrentRootResourceIdentifier().IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void CreateAcquiringResources_GivenMultipleResources_AcquiresTheResourcesInOrderOfResourceIdentifierAscending()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            var childResource = new Resource(child);
            var parentResource = new Resource(parent);
            var competingResourceGroup = ResourceGroup.CreateAcquiringResources(childResource, parentResource);

            // Act
            ResourceGroup resourceGroup = null;
            var thread = new Thread(() => resourceGroup = ResourceGroup.CreateAcquiringResources(childResource, parentResource));
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
        public void CreateAcquiringResources_GivenTheResourceIdentifierChangesAndThatChangesTheOrderInWhichToAcquireResources_CanStillAcquireResourcesEventually()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            var grandParent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(parent);
            var childResource = new Resource(child);
            var parentResource = new Resource(parent);
            var competingResourceGroup = ResourceGroup.CreateAcquiringResources(childResource, parentResource);

            // Act
            ResourceGroup resourceGroup = null;
            var thread = new Thread(() => resourceGroup = ResourceGroup.CreateAcquiringResources(childResource, parentResource));
            thread.Start();
            Thread.Sleep(500);

            // Assert
            child.IsAcquiredBy(competingResourceGroup).Should().BeTrue();
            parent.IsAcquiredBy(competingResourceGroup).Should().BeTrue();

            // Act
            child.SetParentResourceIdentifier(grandParent);
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
        public void ConnectResources_WhenGivenAResourceNotInTheResourceGroup_ThrowsAnArgumentException()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            var resource1 = new Resource(ResourceIdentifier.Create());
            var resource2 = new Resource(ResourceIdentifier.Create());

            // Act
            resourceGroup.ConnectResources(resource1, resource2);
        }

        [Test]
        public void ConnectResources_GivenTheSameResourceTwice_DoesNothing()
        {
            // Arrange
            var resource = new Resource(ResourceIdentifier.Create());
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource);

            // Act
            resourceGroup.ConnectResources(resource, resource);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectResources_GivenTheResourceGroupHasFreedItsResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resource = new Resource(ResourceIdentifier.Create());
            var resourceGroup = ResourceGroup.CreateAcquiringResources(resource);
            resourceGroup.FreeResources();

            // Act
            resourceGroup.ConnectResources(resource, resource);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void CreateAndAcquireResource_GivenTheResourceGroupHasFreedItsResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            resourceGroup.FreeResources();

            // Act
            resourceGroup.CreateAndAcquireResource();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FreeResources_GivenTheResourceGroupHasFreedItsResources_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var resourceGroup = ResourceGroup.CreateWithNoAcquiredResources();
            resourceGroup.FreeResources();

            // Act
            resourceGroup.FreeResources();
        }
    }
}
