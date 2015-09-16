using FluentAssertions;
using NUnit.Framework;
using SharedResources.SharedResources;

namespace SharedResources.Tests.UnitTests.Models.Resources
{
    [TestFixture]
    public class ResourceTests
    {
        [Test]
        public void GetCurrentRootResourceIdentifier_GivenAResourceJustInitialisedWithAResourceIdentifier_ReturnsThatResourceIdentifier()
        {
            // Arrange
            var resourceIdentifier = SharedResourceIdentifier.Create();
            var resource = new SharedResource(resourceIdentifier);

            // Act
            var root = resource.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(resourceIdentifier);
        }

        [Test]
        public void GetCurrentRootResourceIdentifier_GivenItsInitialResourceIdentifierHasAParent_ReturnsThatParent()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            child.SetParentSharedResourceIdentifier(parent);
            var resource = new SharedResource(child);

            // Act
            var root = resource.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(parent);
        }
    }
}
