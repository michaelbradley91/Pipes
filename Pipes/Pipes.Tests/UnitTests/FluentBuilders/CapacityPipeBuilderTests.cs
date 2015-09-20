using FluentAssertions;
using NUnit.Framework;
using System;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class CapacityPipeBuilderTests
    {
        private ICapacityPipeBuilder<int> capacityPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            capacityPipeBuilder = new CapacityPipeBuilder<int>();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithCapacity_GivenANegativeCapacity_ThrowsAnArgumentOutOfRangeException()
        {
            capacityPipeBuilder.WithCapacity(-5);
        }

        [Test]
        public void WithCapacity_GivenASpecificCapacity_CreatesAPipeWithThatCapacity()
        {
            // Arrange
            capacityPipeBuilder.WithCapacity(100);

            // Act
            var pipe = capacityPipeBuilder.Build();

            // Assert
            pipe.Capacity.Should().Be(100);
        }
    }
}