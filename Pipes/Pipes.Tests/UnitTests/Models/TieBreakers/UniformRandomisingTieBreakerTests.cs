using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class UniformRandomisingTieBreakerTests
    {
        private IUniformRandomisingTieBreaker uniformRandomisingTieBreaker;

        [SetUp]
        public void SetUp()
        {
            uniformRandomisingTieBreaker = new UniformRandomisingTieBreaker();
        }

        [Test]
        public void ResolveTie_GivenThereIsOneCompetitor_ReturnsThatCompetitor()
        {
            // Arrange
            const int competitor = 5;

            // Act
            var result = uniformRandomisingTieBreaker.ResolveTie(new[] {competitor});

            // Assert
            result.Should().Be(competitor);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void ResolveTie_GivenThereAreNoCompetitors_ThrowsAnException()
        {
            // Act
            uniformRandomisingTieBreaker.ResolveTie(new int[] {});
        }

        // This test is not guaranteed to succeed but is extremely likely to
        [Test]
        public void ResolveTie_SplitsTheResultsApproximatelyByExpectation()
        {
            // Arrange
            var competitors = new[] {5, 8, 9, 10};

            // Act
            var competitorResults = new List<int>();
            for (var i = 0; i < 12000; i++)
            {
                competitorResults.Add(uniformRandomisingTieBreaker.ResolveTie(competitors));
            }

            // Assert
            for (var i = 0; i < competitors.Length; i++)
            {
                var i1 = i;
                competitorResults.Count(r => r == competitors[i1]).Should().BeLessThan(4000).And.BeGreaterThan(2000);
            }
        }
    }
}
