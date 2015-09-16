using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Resources;

namespace Pipes.Tests.UnitTests.Models.Resources
{
    [TestFixture]
    public class ResourceIdentifierTests
    {
        [Test]
        public void Create_CreatesAResourceIdentifier()
        {
            // Act
            var resourceIdentifier = ResourceIdentifier.Create();

            // Assert
            resourceIdentifier.Should().NotBeNull();
        }

        [Test]
        public void Equals_UsesReferenceEquality()
        {
            // Arrange
            var resourceIdentifier1 = ResourceIdentifier.Create();
            var resourceIdentifier2 = ResourceIdentifier.Create();

            // Act
            // ReSharper disable once EqualExpressionComparison
            var resourceIdentifier1EqualsItself = resourceIdentifier1.Equals(resourceIdentifier1);
            var resourceIdentifier1EqualTo2 = resourceIdentifier1.Equals(resourceIdentifier2);

            // Assert
            resourceIdentifier1EqualsItself.Should().BeTrue();
            resourceIdentifier1EqualTo2.Should().BeFalse();
        }

        [Test]
        public void CreateResourceIdentifierBiggerThan_GivenSeveralResourceIdentifiers_CreatesAResourceIdentifierStrictlyBiggerThanAllOfThem()
        {
            // Arrange
            var resourceIdentifier1 = ResourceIdentifier.Create();
            var resourceIdentifier2 = ResourceIdentifier.CreateResourceIdentifierBiggerThan(resourceIdentifier1);
            var resourceIdentifier3 = ResourceIdentifier.CreateResourceIdentifierBiggerThan(resourceIdentifier2);

            // Act
            var newResourceIdentifier = ResourceIdentifier.CreateResourceIdentifierBiggerThan(resourceIdentifier3, resourceIdentifier1, resourceIdentifier2);

            // Assert
            newResourceIdentifier.Should().BeGreaterThan(resourceIdentifier1);
            newResourceIdentifier.Should().BeGreaterThan(resourceIdentifier2);
            newResourceIdentifier.Should().BeGreaterThan(resourceIdentifier3);
        }

        [Test]
        public void GetCurrentRootResourceIdentifier_GivenAResourceIdentifierWithNoParent_ReturnsItself()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.CreateResourceIdentifierBiggerThan();

            // Act
            var root = resourceIdentifier.GetCurrentRootResourceIdentifier();

            // Assert
            root.Should().Be(resourceIdentifier);
        }

        [Test]
        public void GetCurrentRootResourceIdentifier_GivenAResourceWithAParentAndGrandparent_ReturnsTheGrandparent()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            var grandparent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(parent);

            child.SetParentResourceIdentifier(parent);
            parent.SetParentResourceIdentifier(grandparent);

            // Act
            var root = child.GetCurrentRootResourceIdentifier();

            // Assert
            root.Should().Be(grandparent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void SetParentResourceIdentifier_GivenAResourceIdentifierThatHasAParent_ThrowsAnArgumentException()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            var grandparent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(parent);

            parent.SetParentResourceIdentifier(grandparent);

            // Act
            child.SetParentResourceIdentifier(parent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void SetParentResourceIdentifier_GivenAResourceIdentifierSmallerThanTheChild_ThrowsAnArgumentException()
        {
            // Arrange
            var parent = ResourceIdentifier.Create();
            var child = ResourceIdentifier.CreateResourceIdentifierBiggerThan(parent);

            // Act
            child.SetParentResourceIdentifier(parent);
        }

        [Test]
        public void SetParentResourceIdentifier_GivenAResourceIdentifier_SetsItsRootToBeThatResourceIdentifier()
        {
            // Arrange
            var child = ResourceIdentifier.Create();
            var parent = ResourceIdentifier.CreateResourceIdentifierBiggerThan(child);
            child.SetParentResourceIdentifier(parent);

            // Act
            var root = child.GetCurrentRootResourceIdentifier();

            // Assert
            root.Should().Be(parent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void Free_GivenAResourceIdentifierThatHasNotBeenAcquired_ThrowsAnArgumentException()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resourceGroup = new Mock<IResourceGroup>().Object;

            // Act
            resourceIdentifier.Free(resourceGroup);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void Free_GivenADiffernetResourceGroupToTheOneThatAcquiredIt_ThrowsAnArgumentException()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resourceGroup1 = new Mock<IResourceGroup>().Object;
            var resourceGroup2 = new Mock<IResourceGroup>().Object;
            resourceIdentifier.Acquire(resourceGroup1);

            // Act
            resourceIdentifier.Free(resourceGroup2);
        }

        [Test]
        public void Acquire_GivenAResourceIdentifierThatIsFree_AcquiresThatResourceIdentifier()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resourceGroup = new Mock<IResourceGroup>().Object;

            // Act
            resourceIdentifier.Acquire(resourceGroup);

            // Assert
            resourceIdentifier.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void Acquire_GivenThatTheResourceIsAlreadyAcquired_WillNotSucceedUntilTheResourceIsFree()
        {
            // Arrange
            var resourceIdentifier = ResourceIdentifier.Create();
            var resourceGroup1 = new Mock<IResourceGroup>().Object;
            var resourceGroup2 = new Mock<IResourceGroup>().Object;

            var thread1Finished = false;
            var otherThread = new Thread(() =>
            {
                resourceIdentifier.Acquire(resourceGroup2);
                thread1Finished = true;
            });

            // Act
            resourceIdentifier.Acquire(resourceGroup1);
            otherThread.Start();
            Thread.Sleep(500);
            
            // Assert
            thread1Finished.Should().BeFalse();
            resourceIdentifier.IsAcquiredBy(resourceGroup1).Should().BeTrue();

            // Act
            resourceIdentifier.Free(resourceGroup1);
            Thread.Sleep(500);

            // Assert
            thread1Finished.Should().BeTrue();
            resourceIdentifier.IsAcquiredBy(resourceGroup2).Should().BeTrue();
        }
    }
}
