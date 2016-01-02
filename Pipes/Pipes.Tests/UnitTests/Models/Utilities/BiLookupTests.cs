using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Utilities;

namespace Pipes.Tests.UnitTests.Models.Utilities
{
    [TestFixture]
    public class BiLookupTests
    {
        private IBiLookup<int, string> biLookup;

        [SetUp]
        public void SetUp()
        {
            biLookup = new BiLookup<int, string>();
        }

        [Test]
        public void Add_CanAddASinglePair()
        {
            // Act
            biLookup.Add(1, "1");

            // Assert
            biLookup[1].Should().BeEquivalentTo("1");
            biLookup["1"].Should().BeEquivalentTo(1);
        }

        [Test]
        public void Add_CanAddMultiplePairs()
        {
            // Act
            biLookup.Add(1, "1");
            biLookup.Add(2, "2");

            // Assert
            biLookup[1].Should().BeEquivalentTo("1");
            biLookup["1"].Should().BeEquivalentTo(1);
            biLookup[2].Should().BeEquivalentTo("2");
            biLookup["2"].Should().BeEquivalentTo(2);
        }

        [Test]
        public void Add_CanAddMultipleLeftsForTheSameRight()
        {
            // Act
            biLookup.Add(1, "woo");
            biLookup.Add(2, "woo");

            // Assert
            biLookup[1].Should().BeEquivalentTo("woo");
            biLookup[2].Should().BeEquivalentTo("woo");
            biLookup["woo"].Should().BeEquivalentTo(1, 2);
        }

        [Test]
        public void Add_CanAddMultipleRightsForTheSameLeft()
        {
            // Act
            biLookup.Add(99, "hello");
            biLookup.Add(99, "world");

            // Assert
            biLookup[99].Should().BeEquivalentTo("hello", "world");
            biLookup["hello"].Should().BeEquivalentTo(99);
            biLookup["world"].Should().BeEquivalentTo(99);
        }

        [Test]
        public void Add_CanAddMultipleRightsToMultipleLefts()
        {
            // Act
            biLookup.Add(99, "hello");
            biLookup.Add(99, "world");
            biLookup.Add(70, "hello");
            biLookup.Add(70, "world");

            // Assert
            biLookup[99].Should().BeEquivalentTo("hello", "world");
            biLookup[70].Should().BeEquivalentTo("hello", "world");
            biLookup["hello"].Should().BeEquivalentTo(70, 99);
            biLookup["world"].Should().BeEquivalentTo(70, 99);
        }

        [Test]
        public void LeftIndexer_GivenThereIsNoCorrespondingRight_ReturnsAnEmptyList()
        {
            // Assert
            biLookup[100].Should().BeEmpty();
        }

        [Test]
        public void LeftIndexer_GivenThereIsOneCorrespondingRight_ReturnsThatRight()
        {
            // Arrange
            biLookup.Add(100, "hello");

            // Act
            var rights = biLookup[100];

            // Assert
            rights.Should().BeEquivalentTo("hello");
        }

        [Test]
        public void LeftIndexer_GivenThereAreMultipleCorrespondingRights_ReturnsAllRightsInAList()
        {
            // Arrange
            biLookup.Add(100, "hello");
            biLookup.Add(100, "world");

            // Act
            var rights = biLookup[100];

            // Assert
            rights.Should().BeEquivalentTo("hello", "world");
        }

        [Test]
        public void GetByLeft_GivenThereIsNoCorrespondingRight_ReturnsAnEmptyList()
        {
            // Assert
            biLookup.GetByLeft(100).Should().BeEmpty();
        }

        [Test]
        public void GetByLeft_GivenThereIsOneCorrespondingRight_ReturnsThatRight()
        {
            // Arrange
            biLookup.Add(100, "hello");

            // Act
            var rights = biLookup.GetByLeft(100);

            // Assert
            rights.Should().BeEquivalentTo("hello");
        }

        [Test]
        public void GetByLeft_GivenThereAreMultipleCorrespondingRights_ReturnsAllRightsInAList()
        {
            // Arrange
            biLookup.Add(100, "hello");
            biLookup.Add(100, "world");

            // Act
            var rights = biLookup.GetByLeft(100);

            // Assert
            rights.Should().BeEquivalentTo("hello", "world");
        }

        [Test]
        public void RightIndexer_GivenThereIsNoCorrespondingLeft_ReturnsAnEmptyList()
        {
            // Assert
            biLookup["woo"].Should().BeEmpty();
        }

        [Test]
        public void RightIndexer_GivenThereIsOneCorrespondingLeft_ReturnsThatLeft()
        {
            // Arrange
            biLookup.Add(100, "woo");

            // Act
            var lefts = biLookup["woo"];

            // Assert
            lefts.Should().BeEquivalentTo(100);
        }

        [Test]
        public void RightIndexer_GivenThereAreMultipleCorrespondingLefts_ReturnsAllLeftsInAList()
        {
            // Arrange
            biLookup.Add(100, "woo");
            biLookup.Add(99, "woo");

            // Act
            var lefts = biLookup["woo"];

            // Assert
            lefts.Should().BeEquivalentTo(100, 99);
        }

        [Test]
        public void GetByRight_GivenThereIsNoCorrespondingLeft_ReturnsAnEmptyList()
        {
            // Assert
            biLookup.GetByRight("woo").Should().BeEmpty();
        }

        [Test]
        public void GetByRight_GivenThereIsOneCorrespondingLeft_ReturnsThatLeft()
        {
            // Arrange
            biLookup.Add(100, "woo");

            // Act
            var lefts = biLookup.GetByRight("woo");

            // Assert
            lefts.Should().BeEquivalentTo(100);
        }

        [Test]
        public void GetByRight_GivenThereAreMultipleCorrespondingLefts_ReturnsAllLeftsInAList()
        {
            // Arrange
            biLookup.Add(100, "woo");
            biLookup.Add(99, "woo");

            // Act
            var lefts = biLookup.GetByRight("woo");

            // Assert
            lefts.Should().BeEquivalentTo(99, 100);
        }
    }
}
