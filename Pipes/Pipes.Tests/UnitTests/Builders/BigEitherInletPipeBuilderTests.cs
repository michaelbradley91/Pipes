using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class BigEitherInletPipeBuilderTests
    {
        private IBigEitherInletPipeBuilder<int> bigEitherInletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            bigEitherInletPipeBuilder = new BigEitherInletPipeBuilder<int>();
        }

        [Test]
        public void WithSize_ReturnsABuilder_WithTheGivenSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = bigEitherInletPipeBuilder.WithSize(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WithSize_GivenASizeLessThatOne_ThrowsAnException()
        {
            // Act
            bigEitherInletPipeBuilder.WithSize(0);
        }
    }
}