using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingEitherInletPipeBuilderTests
    {
        [Test]
        public void Build_ReturnsAPipeWithTheTieBreakerPassedIntoTheBuildersConstructor()
        {
            // Arrange
            var tieBreaker = new Mock<ITwoWayTieBreaker>().Object;

            // Act
            var pipe = new TieBreakingEitherInletPipeBuilder<ITwoWayTieBreaker, int>(tieBreaker).Build();

            // Assert
            pipe.GetType().Should().BeAssignableTo<IEitherInletPipe<ITwoWayTieBreaker, int>>();
            pipe.TieBreaker.Should().Be(tieBreaker);
        }
    }
}