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
    }
}
