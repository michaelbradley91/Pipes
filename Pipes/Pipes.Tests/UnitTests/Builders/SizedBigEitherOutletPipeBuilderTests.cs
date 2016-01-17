using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SizedBigEitherOutletPipeBuilderTests
    {
        private ISizedBigEitherOutletPipeBuilder<int> sizedBigEitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sizedBigEitherOutletPipeBuilder = new SizedBigEitherOutletPipeBuilder<int>(3);
        }

        [Test]
        public void New_GivenASize_InitialisesTheBuilderWithThatSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = new SizedBigEitherOutletPipeBuilder<int>(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        public void Build_ReturnsABigEitherOutletPipe_WithTheSetSize()
        {
            // Arrange
            const int size = 5;
            sizedBigEitherOutletPipeBuilder.Size = size;

            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
            pipe.Outlets.Should().HaveCount(size);
            pipe.Outlets.Distinct().Should().HaveCount(size);
        }

        [Test]
        public void Build_ReturnsABigEitherOutletPipe_WithAnInlet()
        {
            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Inlet.Should().NotBeNull();
        }

        [Test]
        public void Build_ReturnsABigEitherOutletPipe_WithATieBreaker()
        {
            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.TieBreaker.Should().NotBeNull();
        }

        [Test]
        public void WithTieBreaker_ReturnsABuilder_WithTheGivenTieBreakerAndPreviousSize()
        {
            // Arrange
            var size = sizedBigEitherOutletPipeBuilder.Size;
            var tieBreaker = new UniformRandomisingTieBreaker();

            // Act
            var builder = sizedBigEitherOutletPipeBuilder.WithTieBreaker(tieBreaker);

            // Assert
            builder.Size.Should().Be(size);
            builder.TieBreaker.Should().Be(tieBreaker);
        }
    }
}