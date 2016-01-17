using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class BigEitherOutletPipeBuilderTests
    {
        private IBigEitherOutletPipeBuilder<int> bigEitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            bigEitherOutletPipeBuilder = new BigEitherOutletPipeBuilder<int>();
        }

        [Test]
        public void WithSize_ReturnsABuilder_WithTheGivenSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = bigEitherOutletPipeBuilder.WithSize(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WithSize_GivenASizeLessThatOne_ThrowsAnException()
        {
            // Act
            bigEitherOutletPipeBuilder.WithSize(0);
        }
    }
}