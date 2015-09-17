using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SharedResources.SharedResources;

namespace SharedResources.Tests.UnitTests.SharedResources
{
    [TestFixture]
    public class ResourceTests
    {
        [Test]
        public void GetCurrentRootResourceIdentifier_GivenAResourceJustInitialisedWithAResourceIdentifier_ReturnsThatResourceIdentifier()
        {
            // Arrange
            var resource = new SharedResource();
            var resourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(resource.GetCurrentRootSharedResourceIdentifier());
            resource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(resourceIdentifier);

            // Act
            var root = resource.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(resourceIdentifier);
        }

        [Test]
        public void GetCurrentRootResourceIdentifier_GivenItsInitialResourceIdentifierHasAParent_ReturnsThatParent()
        {
            // Arrange
            var resource = new SharedResource();
            var child = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(resource.GetCurrentRootSharedResourceIdentifier());
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            resource.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(child);
            child.SetParentSharedResourceIdentifier(parent);

            // Act
            var root = resource.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(parent);
        }

        [Test]
        public void DirectlyConnectedSharedResources_IncludesTheResourceItself()
        {
            // Arrange
            var sharedResource = new SharedResource();

            // Act
            var directlyConnectedSharedResources = sharedResource.DirectlyConnectedSharedResources;

            // Assert
            directlyConnectedSharedResources.Should().HaveCount(1);
            directlyConnectedSharedResources.Should().Contain(sharedResource);
        }

        [Test]
        public void ConnectedSharedResources_IncludesTheResourceItself()
        {
            // Arrange
            var sharedResource = new SharedResource();

            // Act
            var directlyConnectedSharedResources = sharedResource.ConnectedSharedResources;

            // Assert
            directlyConnectedSharedResources.Should().HaveCount(1);
            directlyConnectedSharedResources.Should().Contain(sharedResource);
        }

        [Test]
        public void DirectlyConnect_GivenASharedResource_ConnectsTheCurrentSharedResourceToThatSharedResource()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();

            // Act
            sharedResource1.DirectlyConnect(sharedResource2);

            // Assert
            sharedResource1.DirectlyConnectedSharedResources.Should().Contain(sharedResource2);
            sharedResource1.ConnectedSharedResources.Should().Contain(sharedResource2);
            sharedResource2.DirectlyConnectedSharedResources.Should().Contain(sharedResource1);
            sharedResource2.ConnectedSharedResources.Should().Contain(sharedResource1);
        }

        [Test]
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public void DirectlyConnect_WhenAppliedToResourcesInAChain_ConnectsThemAllTogether()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();
            var sharedResource3 = new SharedResource();
            var sharedResource4 = new SharedResource();
            
            // Act
            sharedResource1.DirectlyConnect(sharedResource2);
            sharedResource2.DirectlyConnect(sharedResource3);
            sharedResource2.DirectlyConnect(sharedResource4);

            // Assert
            sharedResource1.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1, sharedResource2 }.Contains(r));
            sharedResource2.DirectlyConnectedSharedResources.Should().Contain(new [] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
            sharedResource3.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource3, sharedResource2 }.Contains(r));
            sharedResource4.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource4, sharedResource2 }.Contains(r));

            sharedResource2.ConnectedSharedResources.ShouldAllBeEquivalentTo(sharedResource1.ConnectedSharedResources);
            sharedResource3.ConnectedSharedResources.ShouldAllBeEquivalentTo(sharedResource1.ConnectedSharedResources);
            sharedResource4.ConnectedSharedResources.ShouldAllBeEquivalentTo(sharedResource1.ConnectedSharedResources);
            sharedResource1.ConnectedSharedResources.ShouldAllBeEquivalentTo(new[] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void RemoveDirectConnectionTo_GivenTheResourceItself_ThrowsAnArgumentException()
        {
            // Arrange
            var sharedResource = new SharedResource();

            // Act
            sharedResource.RemoveDirectConnectionTo(sharedResource);
        }

        [Test]
        public void RemoveDirectConnectionTo_GivenAPairOfConnectedSharedResources_DisconnectsThemBoth()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();
            sharedResource1.DirectlyConnect(sharedResource2);

            // Act
            sharedResource2.RemoveDirectConnectionTo(sharedResource1);

            // Assert
            sharedResource1.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1 }.Contains(r));
            sharedResource1.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1 }.Contains(r));
            sharedResource2.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource2 }.Contains(r));
            sharedResource2.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource2 }.Contains(r));
        }

        [Test]
        public void RemoveDirectConnectionTo_WhenDisconnectingAChainOfSharedResources_BreaksThatChain()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();
            var sharedResource3 = new SharedResource();
            var sharedResource4 = new SharedResource();
            sharedResource1.DirectlyConnect(sharedResource2);
            sharedResource2.DirectlyConnect(sharedResource3);
            sharedResource3.DirectlyConnect(sharedResource4);

            // Act
            sharedResource2.RemoveDirectConnectionTo(sharedResource3);

            // Assert
            sharedResource2.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1, sharedResource2 }.Contains(r));
            sharedResource3.DirectlyConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource3, sharedResource4 }.Contains(r));

            sharedResource1.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1, sharedResource2 }.Contains(r));
            sharedResource2.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource1, sharedResource2 }.Contains(r));
            sharedResource3.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource3, sharedResource4 }.Contains(r));
            sharedResource4.ConnectedSharedResources.Should().OnlyContain(r => new[] { sharedResource3, sharedResource4 }.Contains(r));
        }

        [Test]
        public void ConnectedSharedResources_GivenTheSharedResourcesAreConnectedInACycle_ResolvesTheCycleCorrectly()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();
            var sharedResource3 = new SharedResource();
            var sharedResource4 = new SharedResource();
            sharedResource1.DirectlyConnect(sharedResource2);
            sharedResource2.DirectlyConnect(sharedResource3);
            sharedResource3.DirectlyConnect(sharedResource4);
            
            // Act
            sharedResource3.DirectlyConnect(sharedResource1);

            // Assert
            sharedResource1.ConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
            sharedResource2.ConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
            sharedResource3.ConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
            sharedResource4.ConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2, sharedResource3, sharedResource4 });
        }

        [Test]
        public void DirectlyConnect_CalledTwiceForTheSameResource_OnlyConnectsThatResourceOnce()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();

            // Act
            sharedResource1.DirectlyConnect(sharedResource2);
            sharedResource1.DirectlyConnect(sharedResource2);

            // Assert
            sharedResource1.ConnectedSharedResources.Should().HaveCount(2);
            sharedResource2.ConnectedSharedResources.Should().HaveCount(2);
            sharedResource2.ConnectedSharedResources.ShouldAllBeEquivalentTo(sharedResource1.ConnectedSharedResources);
            sharedResource1.ConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2 });

            sharedResource1.DirectlyConnectedSharedResources.Should().HaveCount(2);
            sharedResource2.DirectlyConnectedSharedResources.Should().HaveCount(2);
            sharedResource2.DirectlyConnectedSharedResources.ShouldAllBeEquivalentTo(sharedResource1.ConnectedSharedResources);
            sharedResource1.DirectlyConnectedSharedResources.Should().Contain(new[] { sharedResource1, sharedResource2 });
        }

        [Test]
        public void RemoveDirectConnectionTo_GivenAResourceThisIsNotConnectedTo_DoesNothing()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();

            // Act
            sharedResource1.RemoveDirectConnectionTo(sharedResource2);

            // Assert
            sharedResource1.ConnectedSharedResources.Should().HaveCount(1);
            sharedResource1.ConnectedSharedResources.Should().Contain(sharedResource1);
            sharedResource2.ConnectedSharedResources.Should().HaveCount(1);
            sharedResource2.ConnectedSharedResources.Should().Contain(sharedResource2);

            sharedResource1.DirectlyConnectedSharedResources.Should().HaveCount(1);
            sharedResource1.DirectlyConnectedSharedResources.Should().Contain(sharedResource1);
            sharedResource2.DirectlyConnectedSharedResources.Should().HaveCount(1);
            sharedResource2.DirectlyConnectedSharedResources.Should().Contain(sharedResource2);
        }

        [Test]
        public void ResetRootSharedResourceIdentifier_GivenASharedResourceIdentifier_SetsTheRootToBeThatSharedResourceIdentifier()
        {
            // Arrange
            var sharedResource1 = new SharedResource();
            var sharedResource2 = new SharedResource();
            var sharedRoot = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResource1.GetCurrentRootSharedResourceIdentifier(), sharedResource2.GetCurrentRootSharedResourceIdentifier());
            sharedResource1.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(sharedRoot);
            sharedResource2.GetCurrentRootSharedResourceIdentifier().SetParentSharedResourceIdentifier(sharedRoot);
            var newRootForSharedResource1 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedRoot);
            var newRootForSharedResource2 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedRoot);

            // Act
            sharedResource1.ResetRootSharedResourceIdentifier(newRootForSharedResource1);
            sharedResource2.ResetRootSharedResourceIdentifier(newRootForSharedResource2);

            // Assert
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().Be(newRootForSharedResource1);
            sharedResource2.GetCurrentRootSharedResourceIdentifier().Should().Be(newRootForSharedResource2);
            sharedResource1.GetCurrentRootSharedResourceIdentifier().Should().NotBeSameAs(sharedResource2.GetCurrentRootSharedResourceIdentifier());
        }

        [Test]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void AssociatedObject_RemembersTheObjectAssociatedToThisResource()
        {
            // Arrange
            var sharedResource = new SharedResource();
            const int associatedObject = 3;

            // Act
            sharedResource.AssociatedObject = associatedObject;

            // Assert
            sharedResource.AssociatedObject.Should().Be(associatedObject);
        }
    }
}
