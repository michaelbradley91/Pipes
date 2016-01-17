using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SizedBigEitherInletPipeBuilderTests
    {
        private ISizedBigEitherInletPipeBuilder<int> sizedBigEitherInletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sizedBigEitherInletPipeBuilder = new SizedBigEitherInletPipeBuilder<int>(3);
        }

        [Test]
        public void New_GivenASize_InitialisesTheBuilderWithThatSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = new SizedBigEitherInletPipeBuilder<int>(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        public void Build_ReturnsABigEitherInletPipe_WithTheSetSize()
        {
            // Arrange
            const int size = 5;
            sizedBigEitherInletPipeBuilder.Size = size;

            // Act
            var pipe = sizedBigEitherInletPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
            pipe.Inlets.Should().HaveCount(size);
            pipe.Inlets.Distinct().Should().HaveCount(size);
        }

        [Test]
        public void Build_ReturnsABigEitherInletPipe_WithAnOutlet()
        {
            // Act
            var pipe = sizedBigEitherInletPipeBuilder.Build();

            // Assert
            pipe.Outlet.Should().NotBeNull();
        }

        [Test]
        public void Build_ReturnsABigEitherInletPipe_WithATieBreaker()
        {
            // Act
            var pipe = sizedBigEitherInletPipeBuilder.Build();

            // Assert
            pipe.TieBreaker.Should().NotBeNull();
        }

        [Test]
        public void WithTieBreaker_ReturnsABuilder_WithTheGivenTieBreakerAndPreviousSize()
        {
            // Arrange
            var size = sizedBigEitherInletPipeBuilder.Size;
            var tieBreaker = new UniformRandomisingTieBreaker();

            // Act
            var builder = sizedBigEitherInletPipeBuilder.WithTieBreaker(tieBreaker);

            // Assert
            builder.Size.Should().Be(size);
            builder.TieBreaker.Should().Be(tieBreaker);
        }
    }
}