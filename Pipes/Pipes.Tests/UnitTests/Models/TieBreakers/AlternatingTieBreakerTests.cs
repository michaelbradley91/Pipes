using FluentAssertions;
using NUnit.Framework;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class AlternatingTieBreakerTests
    {
        [Test]
        public void GivenLeftHasPriorityInitially_SetsNextPriorityAndInitialPriorityToLeft()
        {
            // Act
            var tieBreaker = new AlternatingTieBreaker(Alternated.LeftHasPriorityInitially);

            // Assert
            tieBreaker.InitialPriority.Should().Be(Priority.Left);
            tieBreaker.NextPriority.Should().Be(Priority.Left);
        }

        [Test]
        public void GivenRightHasPriorityInitially_SetsNextPriorityAndInitialPriorityToRight()
        {
            // Act
            var tieBreaker = new AlternatingTieBreaker(Alternated.RightHasPriorityInitially);

            // Assert
            tieBreaker.InitialPriority.Should().Be(Priority.Right);
            tieBreaker.NextPriority.Should().Be(Priority.Right);
        }

        [Test]
        public void DeepCopy_CopiesTheNextAndInitialPriorityCorrectly()
        {
            // Arrange
            var tieBreaker = new AlternatingTieBreaker(Alternated.RightHasPriorityInitially);

            // Act
            var copy = (AlternatingTieBreaker)tieBreaker.DeepCopy();

            // Assert
            copy.InitialPriority.Should().Be(tieBreaker.InitialPriority);
            copy.NextPriority.Should().Be(tieBreaker.NextPriority);
        }

        [Test]
        public void ResolveTie_AlternatesThePriorityCorrectly()
        {
            // Arrange
            var tieBreaker = new AlternatingTieBreaker(Alternated.RightHasPriorityInitially);

            // Act
            var firstResult = tieBreaker.ResolveTie();
            var secondresult = tieBreaker.ResolveTie();
            var thirdResult = tieBreaker.ResolveTie();

            // Assert
            firstResult.Should().Be(TieResult.Right);
            secondresult.Should().Be(TieResult.Left);
            thirdResult.Should().Be(TieResult.Right);
        }
    }
}
