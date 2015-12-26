using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Constants;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class EitherInletPipeBuilderTests
    {
        private IEitherInletPipeBuilder<int> eitherInletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            eitherInletPipeBuilder = new EitherInletPipeBuilder<int>();
        }

        [Test]
        public void Build_ReturnsAPipeWithAPrioritisingTieBreaker_WithPriorityLeft()
        {
            // Act
            var pipe = eitherInletPipeBuilder.Build();

            // Assert
            pipe.GetType().Should().BeAssignableTo<IEitherInletPipe<IPrioritisingTieBreaker, int>>();
            pipe.TieBreaker.Priority.Should().Be(Priority.Left);
        }

        [Test]
        public void WithTieBreaker_GivenASpecificTieBreaker_ReturnsAPipeWithThatTieBreaker()
        {
            // Arrange
            var tieBreaker = new Mock<ITieBreaker>();

            // Act
            var pipe = eitherInletPipeBuilder.WithTieBreaker(tieBreaker.Object).Build();

            // Assert
            pipe.TieBreaker.Should().Be(tieBreaker.Object);
        }

        [Test]
        public void WithAlternatingTieBreaker_GivenNoSpecificAlternation_DefaultsToLeftInitialPriority()
        {
            // Act
            var pipe = eitherInletPipeBuilder.WithAlternatingTieBreaker().Build();

            // Assert
            pipe.TieBreaker.InitialPriority.Should().Be(Priority.Left);
        }

        [Test]
        public void WithAlternatingTieBreaker_GivenASpecificAlternation_ReturnsAPipeWithThatAlternation()
        {
            // Arrange
            const Alternated alternation = Alternated.RightHasPriorityInitially;

            // Act
            var pipe = eitherInletPipeBuilder.WithAlternatingTieBreaker(alternation).Build();

            // Assert
            pipe.TieBreaker.InitialPriority.Should().Be(Priority.Right);
        }

        [Test]
        public void WithPrioritisingTieBreaker_GivenNoSpecificPriority_DefaultsToLeftPriority()
        {
            // Act
            var pipe = eitherInletPipeBuilder.WithPrioritisingTieBreaker().Build();

            // Assert
            pipe.TieBreaker.Priority.Should().Be(Priority.Left);
        }

        [Test]
        public void WithPrioritisingTieBreaker_GivenASpecificPriority_ReturnsAPipeWithThatPriority()
        {
            // Arrange
            const Priority priority = Priority.Right;

            // Act
            var pipe = eitherInletPipeBuilder.WithPrioritisingTieBreaker(priority).Build();

            // Assert
            pipe.TieBreaker.Priority.Should().Be(Priority.Right);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            // Act
            eitherInletPipeBuilder.WithRandomisingTieBreaker(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            // Act
            eitherInletPipeBuilder.WithRandomisingTieBreaker(2);
        }

        [Test]
        public void WithRandomisingTieBreaker_GivenNoSpecificProbability_DefaultsToFiftyPercentProbability()
        {
            // Act
            var pipe = eitherInletPipeBuilder.WithRandomisingTieBreaker().Build();

            // Assert
            pipe.TieBreaker.LeftProbability.Should().Be(0.5);
            pipe.TieBreaker.LeftProbability.Should().Be(pipe.TieBreaker.RightProbability);
        }

        [Test]
        public void WithRandomisingTieBreaker_GivenASpecificLeftProbability_ReturnsAPipeWithThatLeftProbability()
        {
            // Arrange
            const double leftProbability = 0.6;

            // Act
            var pipe = eitherInletPipeBuilder.WithRandomisingTieBreaker(leftProbability).Build();

            // Assert
            pipe.TieBreaker.LeftProbability.Should().Be(leftProbability);
            pipe.TieBreaker.RightProbability.Should().Be(1 - leftProbability);
        }
    }
}