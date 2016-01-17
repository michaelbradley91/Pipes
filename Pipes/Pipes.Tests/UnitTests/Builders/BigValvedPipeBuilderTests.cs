using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class BigValvedPipeBuilderTests
    {
        private IBigValvedPipeBuilder<string, int> bigValvedPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            bigValvedPipeBuilder = new BigValvedPipeBuilder<string, int>();
        }

        [Test]
        public void WithSize_ReturnsABuilder_WithTheGivenSize()
        {
            // Arrange
            const int numberOfInlets = 4;
            const int numberOfOutlets = 3;

            // Act
            var builder = bigValvedPipeBuilder.WithSize(numberOfInlets, numberOfOutlets);

            // Assert
            builder.NumberOfInlets.Should().Be(numberOfInlets);
            builder.NumberOfOutlets.Should().Be(numberOfOutlets);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WithSize_GivenANumberOfInletsLessThanOne_ThrowsAnException()
        {
            // Act
            bigValvedPipeBuilder.WithSize(0, 4);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WithSize_GivenANumberOfOutletsLessThanOne_ThrowsAnException()
        {
            // Act
            bigValvedPipeBuilder.WithSize(4, 0);
        }
    }
}