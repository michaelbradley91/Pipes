using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TieBreakingEitherOutletPipeBuilderTests
    {
        [Test]
        public void Build_ReturnsAPipeWithTheTieBreakerPassedIntoTheBuildersConstructor()
        {
            // Arrange
            var tieBreaker = new Mock<ITieBreaker>().Object;

            // Act
            var pipe = new TieBreakingEitherOutletPipeBuilder<ITieBreaker, int>(tieBreaker).Build();

            // Assert
            pipe.GetType().Should().BeAssignableTo<IEitherOutletPipe<ITieBreaker, int>>();
            pipe.TieBreaker.Should().Be(tieBreaker);
        }
    }
}