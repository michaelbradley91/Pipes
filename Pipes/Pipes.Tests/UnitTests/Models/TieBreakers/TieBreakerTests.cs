using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class TieBreakerTests
    {
        private DummyTieBreaker tieBreaker;

        [SetUp]
        public void SetUp()
        {
            tieBreaker = new DummyTieBreaker();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ResolveTie_GivenZeroCompetitors_ThrowsAnException()
        {
            // Arrange
            var competitors = new int[0];

            // Act
            tieBreaker.ResolveTie(competitors);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [Test]
        public void ResolveTie_GivenNullForCompetitors_ThrowsAnException()
        {
            // Act
            tieBreaker.ResolveTie(null);
        }

        [Test]
        public void ResolveTie_GivenOneCompetitor_ReturnsThatCompetitor()
        {
            // Arrange
            const int competitor = 3;
            var competitors = new[] { competitor };

            // Act
            var result = tieBreaker.ResolveTie(competitors);

            // Assert
            result.Should().Be(competitor);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ResolveTie_GivenDuplicateCompetitors_ThrowsAnException()
        {
            // Arrange
            var competitors = new[] {3, 3};

            // Act
            tieBreaker.ResolveTie(competitors);
        }

        [Test]
        public void ResolveTie_GivenMultipleCompetitors_CallsItsImplementationsResolveMethod()
        {
            // Arrange
            var competitors = new[] {3, 4, 5};

            // Act
            var result = tieBreaker.ResolveTie(competitors);

            // Assert
            tieBreaker.PassedCompetitors.ShouldAllBeEquivalentTo(competitors);
            result.Should().Be(DummyTieBreaker.TieResult);
        }

        private class DummyTieBreaker : TieBreaker
        {
            public int[] PassedCompetitors;
            public const int TieResult = -1;

            protected override int ResolveTie(int[] tiedCompetitors)
            {
                PassedCompetitors = tiedCompetitors;
                return TieResult;
            }
        }
    }
}
