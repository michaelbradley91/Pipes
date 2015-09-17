using System;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SharedResources.SharedResources;

namespace SharedResources.Tests.UnitTests.SharedResources
{
    [TestFixture]
    public class SharedResourceIdentifierTests
    {
        [Test]
        public void Create_CreatesASharedsharedResourceIdentifier()
        {
            // Act
            var sharedResourceIdentifier = SharedResourceIdentifier.Create();

            // Assert
            sharedResourceIdentifier.Should().NotBeNull();
        }

        [Test]
        public void Equals_UsesReferenceEquality()
        {
            // Arrange
            var sharedResourceIdentifier1 = SharedResourceIdentifier.Create();
            var sharedResourceIdentifier2 = SharedResourceIdentifier.Create();

            // Act
            // ReSharper disable once EqualExpressionComparison
            var sharedResourceIdentifier1EqualsItself = sharedResourceIdentifier1.Equals(sharedResourceIdentifier1);
            var sharedResourceIdentifier1EqualTo2 = sharedResourceIdentifier1.Equals(sharedResourceIdentifier2);

            // Assert
            sharedResourceIdentifier1EqualsItself.Should().BeTrue();
            sharedResourceIdentifier1EqualTo2.Should().BeFalse();
        }

        [Test]
        public void CreatesharedResourceIdentifierBiggerThan_GivenSeveralsharedResourceIdentifiers_CreatesAsharedResourceIdentifierStrictlyBiggerThanAllOfThem()
        {
            // Arrange
            var sharedResourceIdentifier1 = SharedResourceIdentifier.Create();
            var sharedResourceIdentifier2 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResourceIdentifier1);
            var sharedResourceIdentifier3 = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResourceIdentifier2);

            // Act
            var newsharedResourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(sharedResourceIdentifier3, sharedResourceIdentifier1, sharedResourceIdentifier2);

            // Assert
            newsharedResourceIdentifier.Should().BeGreaterThan(sharedResourceIdentifier1);
            newsharedResourceIdentifier.Should().BeGreaterThan(sharedResourceIdentifier2);
            newsharedResourceIdentifier.Should().BeGreaterThan(sharedResourceIdentifier3);
        }

        [Test]
        public void GetCurrentRootsharedResourceIdentifier_GivenAsharedResourceIdentifierWithNoParent_ReturnsItself()
        {
            // Arrange
            var sharedResourceIdentifier = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan();

            // Act
            var root = sharedResourceIdentifier.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(sharedResourceIdentifier);
        }

        [Test]
        public void GetCurrentRootsharedResourceIdentifier_GivenAResourceWithAParentAndGrandparent_ReturnsTheGrandparent()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            var grandparent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(parent);

            child.SetParentSharedResourceIdentifier(parent);
            parent.SetParentSharedResourceIdentifier(grandparent);

            // Act
            var root = child.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(grandparent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void SetParentsharedResourceIdentifier_GivenASharedResourceIdentifierThatHasAParent_ThrowsAnArgumentException()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            var grandparent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(parent);

            parent.SetParentSharedResourceIdentifier(grandparent);

            // Act
            child.SetParentSharedResourceIdentifier(parent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void SetParentsharedResourceIdentifier_GivenAsharedResourceIdentifierSmallerThanTheChild_ThrowsAnArgumentException()
        {
            // Arrange
            var parent = SharedResourceIdentifier.Create();
            var child = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(parent);

            // Act
            child.SetParentSharedResourceIdentifier(parent);
        }

        [Test]
        public void SetParentsharedResourceIdentifier_GivenAsharedResourceIdentifier_SetsItsRootToBeThatsharedResourceIdentifier()
        {
            // Arrange
            var child = SharedResourceIdentifier.Create();
            var parent = SharedResourceIdentifier.CreateSharedResourceIdentifierBiggerThan(child);
            child.SetParentSharedResourceIdentifier(parent);

            // Act
            var root = child.GetCurrentRootSharedResourceIdentifier();

            // Assert
            root.Should().Be(parent);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void Free_GivenAsharedResourceIdentifierThatHasNotBeenAcquired_ThrowsAnArgumentException()
        {
            // Arrange
            var sharedResourceIdentifier = SharedResourceIdentifier.Create();
            var resourceGroup = new Mock<ISharedResourceGroup>().Object;

            // Act
            sharedResourceIdentifier.Free(resourceGroup);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void Free_GivenADiffernetResourceGroupToTheOneThatAcquiredIt_ThrowsAnArgumentException()
        {
            // Arrange
            var sharedResourceIdentifier = SharedResourceIdentifier.Create();
            var resourceGroup1 = new Mock<ISharedResourceGroup>().Object;
            var resourceGroup2 = new Mock<ISharedResourceGroup>().Object;
            sharedResourceIdentifier.Acquire(resourceGroup1);

            // Act
            sharedResourceIdentifier.Free(resourceGroup2);
        }

        [Test]
        public void Acquire_GivenAsharedResourceIdentifierThatIsFree_AcquiresThatsharedResourceIdentifier()
        {
            // Arrange
            var sharedResourceIdentifier = SharedResourceIdentifier.Create();
            var resourceGroup = new Mock<ISharedResourceGroup>().Object;

            // Act
            sharedResourceIdentifier.Acquire(resourceGroup);

            // Assert
            sharedResourceIdentifier.IsAcquiredBy(resourceGroup).Should().BeTrue();
        }

        [Test]
        public void Acquire_GivenThatTheResourceIsAlreadyAcquired_WillNotSucceedUntilTheResourceIsFree()
        {
            // Arrange
            var sharedResourceIdentifier = SharedResourceIdentifier.Create();
            var resourceGroup1 = new Mock<ISharedResourceGroup>().Object;
            var resourceGroup2 = new Mock<ISharedResourceGroup>().Object;

            var thread1Finished = false;
            var otherThread = new Thread(() =>
            {
                sharedResourceIdentifier.Acquire(resourceGroup2);
                thread1Finished = true;
            });

            // Act
            sharedResourceIdentifier.Acquire(resourceGroup1);
            otherThread.Start();
            Thread.Sleep(500);
            
            // Assert
            thread1Finished.Should().BeFalse();
            sharedResourceIdentifier.IsAcquiredBy(resourceGroup1).Should().BeTrue();

            // Act
            sharedResourceIdentifier.Free(resourceGroup1);
            Thread.Sleep(500);

            // Assert
            thread1Finished.Should().BeTrue();
            sharedResourceIdentifier.IsAcquiredBy(resourceGroup2).Should().BeTrue();
        }
    }
}
