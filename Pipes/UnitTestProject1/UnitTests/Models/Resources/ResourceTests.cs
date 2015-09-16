using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Resources;

namespace Pipes.Tests.UnitTests.Models.Resources
{
    [TestFixture]
    public class ResourceTests
    {
        [Test]
        public void GetCurrentRootResourceIdentifier_GivenAResourceJustInitialisedWithAResourceIdentifier_ReturnsThatResourceIdentifier()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resource = new Resource(resourceIdentifier);

            // Act
            var root = resource.GetCurrentRootResourceIdentifier();

            // Assert
            root.Should().Be(resourceIdentifier);
        }

        [Test]
        public void GetCurrentRootResourceIdentifier_GivenItsInitialResourceIdentifierHasAParent_ReturnsThatParent()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            child.SetParentResourceIdentifier(parent);
            var resource = new Resource(child);

            // Act
            var root = resource.GetCurrentRootResourceIdentifier();

            // Assert
            root.Should().Be(parent);
        }
    }
}
