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

        [Test]
        public void DirectlyConnectedSharedResources_IncludesTheResourceItself()
        {
            // Arrange
            var sharedResource = new SharedResource(SharedResourceIdentifier.Create());

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
            var sharedResource = new SharedResource(SharedResourceIdentifier.Create());

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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());

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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource3 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource4 = new SharedResource(SharedResourceIdentifier.Create());
            
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
            var sharedResource = new SharedResource(SharedResourceIdentifier.Create());

            // Act
            sharedResource.RemoveDirectConnectionTo(sharedResource);
        }

        [Test]
        public void RemoveDirectConnectionTo_GivenAPairOfConnectedSharedResources_DisconnectsThemBoth()
        {
            // Arrange
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());
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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource3 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource4 = new SharedResource(SharedResourceIdentifier.Create());
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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource3 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource4 = new SharedResource(SharedResourceIdentifier.Create());
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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());

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
            var sharedResource1 = new SharedResource(SharedResourceIdentifier.Create());
            var sharedResource2 = new SharedResource(SharedResourceIdentifier.Create());

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
    }
}
