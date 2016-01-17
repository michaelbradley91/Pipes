using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingSizedBigValvedPipeBuilderTests
    {
        private ITieBreakingSizedBigValvedPipeBuilder<int, string, IMinPrioritisingTieBreaker> tieBreakingSizedBigEitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            tieBreakingSizedBigEitherOutletPipeBuilder = new TieBreakingSizedBigValvedPipeBuilder<int, string, IMinPrioritisingTieBreaker>(3, 4, tieBreaker);
        }

        [Test]
        public void New_GivenSizeAndATieBreaker_SetsTheBuildersCorrespondingProperties()
        {
            // Arrange
            var tieBreaker = new UniformRandomisingTieBreaker();
            const int numberOfInlets = 5;
            const int numberOfOutlets = 6;

            // Act
            var builder = new TieBreakingSizedBigValvedPipeBuilder<int, string, IUniformRandomisingTieBreaker>(numberOfInlets, numberOfOutlets, tieBreaker);

            // Assert
            builder.NumberOfInlets.Should().Be(numberOfInlets);
            builder.NumberOfOutlets.Should().Be(numberOfOutlets);
            builder.TieBreaker.Should().Be(tieBreaker);
        }

        [Test]
        public void Build_CreatesAPipe_BasedOnTheBuildersProperties()
        {
            // Arrange
            const int numberOfInlets = 9;
            const int numberOfOutlets = 1;
            var tieBreaker = new MinPrioritisingTieBreaker();
            tieBreakingSizedBigEitherOutletPipeBuilder.NumberOfInlets = numberOfInlets;
            tieBreakingSizedBigEitherOutletPipeBuilder.NumberOfOutlets = numberOfOutlets;
            tieBreakingSizedBigEitherOutletPipeBuilder.TieBreaker = tieBreaker;

            // Act
            var pipe = tieBreakingSizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Inlets.Should().HaveCount(numberOfInlets);
            pipe.Inlets.Distinct().Should().HaveCount(numberOfInlets);
            pipe.Outlets.Should().HaveCount(numberOfOutlets);
            pipe.Outlets.Distinct().Should().HaveCount(numberOfOutlets);
            pipe.TieBreaker.Should().Be(tieBreaker);
            pipe.Valve.Should().NotBeNull();
        }
    }
}