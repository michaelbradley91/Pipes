using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class BigSplittingPipeBuilderTests
    {
        private IBigSplittingPipeBuilder<int> bigSplittingPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            bigSplittingPipeBuilder = new BigSplittingPipeBuilder<int>();
        }

        [Test]
        public void WithSize_ReturnsABuilder_WithTheGivenSize()
        {
            // Arrange
            const int size = 4;

            // Act
            var builder = bigSplittingPipeBuilder.WithSize(size);

            // Assert
            builder.Size.Should().Be(size);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WithSize_GivenASizeLessThatOne_ThrowsAnException()
        {
            // Act
            bigSplittingPipeBuilder.WithSize(0);
        }
    }
}