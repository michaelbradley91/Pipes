using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SizedBigValvedPipeBuilderTests
    {
        private ISizedBigValvedPipeBuilder<string, int> sizedBigEitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sizedBigEitherOutletPipeBuilder = new SizedBigValvedPipeBuilder<string, int>(3, 4);
        }

        [Test]
        public void New_GivenANumberOfInletsAndOutlets_InitialisesTheBuilderWithThoseValues()
        {
            // Arrange
            const int numberOfInlets = 4;
            const int numberOfOutlets = 5;

            // Act
            var builder = new SizedBigValvedPipeBuilder<string, int>(numberOfInlets, numberOfOutlets);

            // Assert
            builder.NumberOfInlets.Should().Be(numberOfInlets);
            builder.NumberOfOutlets.Should().Be(numberOfOutlets);
        }

        [Test]
        public void Build_ReturnsABigSplittingPipe_WithTheSetNumberOfInletsAndOutlets()
        {
            // Arrange
            const int numberOfInlets = 5;
            const int numberOfOutlets = 4;
            sizedBigEitherOutletPipeBuilder.NumberOfInlets = numberOfInlets;
            sizedBigEitherOutletPipeBuilder.NumberOfOutlets = numberOfOutlets;

            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
            pipe.Inlets.Should().HaveCount(numberOfInlets);
            pipe.Inlets.Distinct().Should().HaveCount(numberOfInlets);
            pipe.Outlets.Should().HaveCount(numberOfOutlets);
            pipe.Outlets.Distinct().Should().HaveCount(numberOfOutlets);
        }
        
        [Test]
        public void Build_ReturnsABigSplittingPipe_WithATieBreaker()
        {
            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.TieBreaker.Should().NotBeNull();
        }

        [Test]
        public void Build_ReturnsABigSplittingPipe_WithAValve()
        {
            // Act
            var pipe = sizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Valve.Should().NotBeNull();
        }

        [Test]
        public void WithTieBreaker_ReturnsABuilder_WithTheGivenTieBreakerAndPreviousNumberOfInletsAndOutlets()
        {
            // Arrange
            var numberOfInlets = sizedBigEitherOutletPipeBuilder.NumberOfInlets;
            var numberOfOutlets = sizedBigEitherOutletPipeBuilder.NumberOfOutlets;
            var tieBreaker = new UniformRandomisingTieBreaker();

            // Act
            var builder = sizedBigEitherOutletPipeBuilder.WithTieBreaker(tieBreaker);

            // Assert
            builder.NumberOfInlets.Should().Be(numberOfInlets);
            builder.NumberOfOutlets.Should().Be(numberOfOutlets);
            builder.TieBreaker.Should().Be(tieBreaker);
        }
    }
}