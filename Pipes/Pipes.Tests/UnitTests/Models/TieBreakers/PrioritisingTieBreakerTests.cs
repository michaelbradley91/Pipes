using FluentAssertions;
using NUnit.Framework;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class PrioritisingTieBreakerTests
    {
        [Test]
        public void GivenPriorityLeft_CreatesATieBreakerWithLeftPriority()
        {
            // Act
            var tieBreaker = new PrioritisingTieBreaker(Priority.Left);

            // Assert
            tieBreaker.Priority.Should().Be(Priority.Left);
        }

        [Test]
        public void GivenPriorityRight_CreatesATieBreakerWithRightPriority()
        {
            // Act
            var tieBreaker = new PrioritisingTieBreaker(Priority.Right);

            // Assert
            tieBreaker.Priority.Should().Be(Priority.Right);
        }

        [Test]
        public void ResolveTie_AlwaysResolvesToThePrioritisedSide()
        {
            // Arrange
            var tieBreaker = new PrioritisingTieBreaker(Priority.Right);

            // Act
            var firstResult = tieBreaker.ResolveTie();
            var secondResult = tieBreaker.ResolveTie();
            var thirdResult = tieBreaker.ResolveTie();

            // Assert
            firstResult.Should().Be(TieResult.Right);
            secondResult.Should().Be(TieResult.Right);
            thirdResult.Should().Be(TieResult.Right);
        }
    }
}
