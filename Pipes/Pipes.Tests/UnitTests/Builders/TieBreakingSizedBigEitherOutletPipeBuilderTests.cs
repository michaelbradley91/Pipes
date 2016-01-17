using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingSizedBigEitherOutletPipeBuilderTests
    {
        private ITieBreakingSizedBigEitherOutletPipeBuilder<int, IMinPrioritisingTieBreaker> tieBreakingSizedBigEitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            var tieBreaker = new MinPrioritisingTieBreaker();
            tieBreakingSizedBigEitherOutletPipeBuilder = new TieBreakingSizedBigEitherOutletPipeBuilder<int, IMinPrioritisingTieBreaker>(3, tieBreaker);
        }

        [Test]
        public void New_GivenSizeAndATieBreaker_SetsTheBuildersCorrespondingProperties()
        {
            // Arrange
            var tieBreaker = new UniformRandomisingTieBreaker();
            const int size = 5;

            // Act
            var builder = new TieBreakingSizedBigEitherOutletPipeBuilder<int, IUniformRandomisingTieBreaker>(size, tieBreaker);

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
            tieBreakingSizedBigEitherOutletPipeBuilder.Size = size;
            tieBreakingSizedBigEitherOutletPipeBuilder.TieBreaker = tieBreaker;

            // Act
            var pipe = tieBreakingSizedBigEitherOutletPipeBuilder.Build();

            // Assert
            pipe.Inlet.Should().NotBeNull();
            pipe.Outlets.Should().HaveCount(size);
            pipe.Outlets.Distinct().Should().HaveCount(size);
            pipe.TieBreaker.Should().Be(tieBreaker);
        }
    }
}