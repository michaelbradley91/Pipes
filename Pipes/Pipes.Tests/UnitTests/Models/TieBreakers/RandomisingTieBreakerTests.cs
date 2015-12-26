using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class RandomisingTieBreakerTests
    {
        [Test]
        public void GivenLeftProbability_CreatesATieBreakerWithThatLeftProbability()
        {
            // Act
            const double leftProbability = 0.643;
            var tieBreaker = new RandomisingTieBreaker(leftProbability);

            // Assert
            tieBreaker.LeftProbability.Should().Be(leftProbability);
            tieBreaker.RightProbability.Should().Be(1 - leftProbability);
        }

        // This test is not guaranteed to succeed but is extremely likely to
        [Test]
        public void ResolveTie_SplitsTheResultsApproximatelyByExpectation()
        {
            // Arrange
            var tieBreaker = new RandomisingTieBreaker(0.3);

            // Act
            var leftResultCount = 0;
            var rightResultCount = 0;
            for (var i = 0; i < 10000; i++)
            {
                switch (tieBreaker.ResolveTie())
                {
                    case TieResult.Left:
                        leftResultCount++;
                        break;
                    case TieResult.Right:
                        rightResultCount++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Assert
            leftResultCount.Should().BeLessThan(4000).And.BeGreaterThan(2000);
            rightResultCount.Should().BeLessThan(8000).And.BeGreaterThan(6000);
        }
    }
}
