using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingSizedBigEitherInletPipeBuilderTests
    {
        private ITieBreakingSizedBigEitherInletPipeBuilder<int, IMinPrioritisingTieBreaker> tieBreakingSizedBigEitherInletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            tieBreakingSizedBigEitherInletPipeBuilder = new TieBreakingSizedBigEitherInletPipeBuilder<int, IMinPrioritisingTieBreaker>(3, tieBreaker);
        }

        [Test]
        public void New_GivenSizeAndATieBreaker_SetsTheBuildersCorrespondingProperties()
        {
            // Arrange
            var tieBreaker = new UniformRandomisingTieBreaker();
            const int size = 5;

            // Act
            var builder = new TieBreakingSizedBigEitherInletPipeBuilder<int, IUniformRandomisingTieBreaker>(size, tieBreaker);

            // Assert
            builder.Size.Should().Be(size);
            builder.TieBreaker.Should().Be(tieBreaker);
        }

        [Test]
        public void Build_CreatesAPipe_BasedOnTheBuildersProperties()
        {
            // Arrange
            const int size = 9;
            var tieBreaker = new MinPrioritisingTieBreaker();
            tieBreakingSizedBigEitherInletPipeBuilder.Size = size;
            tieBreakingSizedBigEitherInletPipeBuilder.TieBreaker = tieBreaker;

            // Act
            var pipe = tieBreakingSizedBigEitherInletPipeBuilder.Build();

            // Assert
            pipe.Inlets.Should().HaveCount(size);
            pipe.Inlets.Distinct().Should().HaveCount(size);
            pipe.Outlet.Should().NotBeNull();
            pipe.TieBreaker.Should().Be(tieBreaker);
        }
    }
}