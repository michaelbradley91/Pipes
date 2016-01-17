using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class MinPrioritisingTieBreakerTests
    {
        private IMinPrioritisingTieBreaker minPrioritisingTieBreaker;

        [SetUp]
        public void SetUp()
        {
            minPrioritisingTieBreaker = new MinPrioritisingTieBreaker();
        }

        [Test]
        public void ResolveTie_GivenThereIsOneCompetitor_ReturnsThatCompetitor()
        {
            // Arrange
            const int competitor = 5;

            // Act
            var result = minPrioritisingTieBreaker.ResolveTie(new[] {competitor});

            // Assert
            result.Should().Be(competitor);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void ResolveTie_GivenThereAreNoCompetitors_ThrowsAnException()
        {
            // Act
            minPrioritisingTieBreaker.ResolveTie(new int[] {});
        }

        // This test is not guaranteed to succeed but is extremely likely to
        [Test]
        public void ResolveTie_GivenMultipleCompetitors_ReturnsTheMinimumCompetitor()
        {
            // Arrange
            var competitors = new[] {3, 6, 8, 1, 9};
            var minCompetitor = competitors.Min();

            // Act
            var result = minPrioritisingTieBreaker.ResolveTie(competitors);

            // Assert
            result.Should().Be(minCompetitor);
        }
    }
}
