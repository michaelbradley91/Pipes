using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Constants;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Models.TieBreakers
{
    [TestFixture]
    public class TwoWayTieBreakerTests
    {
        private DummyTwoWayTieBreaker twoWayTieBreaker;

        [SetUp]
        public void SetUp()
        {
            twoWayTieBreaker = new DummyTwoWayTieBreaker();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ResolveTie_GivenMoreThanTwoCompetitors_ThrowsAnException()
        {
            // Arrange
            var competitors = new[] {1, 2, 3};

            // Act
            twoWayTieBreaker.ResolveTie(competitors);
        }

        [Test]
        public void ResolveTie_GivenTwoCompetitorsAndTheTieResultIsLeft_ReturnsTheSmallerCompetitor()
        {
            // Arrange
            var firstCompetitors = new[] {1, 2};
            var secondCompetitors = new[] {2, 1};

            // Act
            twoWayTieBreaker.Result = TieResult.Left;
            var firstResult = twoWayTieBreaker.ResolveTie(firstCompetitors);
            var secondResult = twoWayTieBreaker.ResolveTie(secondCompetitors);

            // Arrange
            firstResult.Should().Be(1);
            secondResult.Should().Be(1);
        }

        [Test]
        public void ResolveTie_GivenTwoCompetitorsAndTheTieResultIsRight_ReturnsTheLargerCompetitor()
        {
            // Arrange
            var firstCompetitors = new[] { 1, 2 };
            var secondCompetitors = new[] { 2, 1 };

            // Act
            twoWayTieBreaker.Result = TieResult.Right;
            var firstResult = twoWayTieBreaker.ResolveTie(firstCompetitors);
            var secondResult = twoWayTieBreaker.ResolveTie(secondCompetitors);

            // Arrange
            firstResult.Should().Be(2);
            secondResult.Should().Be(2);
        }


        [Test]
        public void ResolveTie_GivenOneCompetitor_ReturnsThatCompetitor()
        {
            // Arrange
            const int competitor = 3;
            var competitors = new[] { competitor };

            // Act
            var result = twoWayTieBreaker.ResolveTie(competitors);

            // Assert
            result.Should().Be(competitor);
        }

        private class DummyTwoWayTieBreaker : TwoWayTieBreaker
        {
            public TieResult Result { private get; set; }
            
            public override TieResult ResolveTie()
            {
                return Result;
            }
        }
    }
}
