using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SizedBigSplittingPipeBuilderTests
    {
        private ISizedBigSplittingPipeBuilder<int> sizedBigSplittingPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sizedBigSplittingPipeBuilder = new SizedBigSplittingPipeBuilder<int>(3);
        }

        [Test]
        public void New_GivenASize_InitialisesTheBuilderWithThatSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = new SizedBigSplittingPipeBuilder<int>(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        public void Build_ReturnsABigSplittingPipe_WithTheSetSize()
        {
            // Arrange
            const int size = 5;
            sizedBigSplittingPipeBuilder.Size = size;

            // Act
            var pipe = sizedBigSplittingPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
            pipe.Outlets.Should().HaveCount(size);
            pipe.Outlets.Distinct().Should().HaveCount(size);
        }

        [Test]
        public void Build_ReturnsABigSplittingPipe_WithAnInlet()
        {
            // Act
            var pipe = sizedBigSplittingPipeBuilder.Build();

            // Assert
            pipe.Inlet.Should().NotBeNull();
        }
    }
}