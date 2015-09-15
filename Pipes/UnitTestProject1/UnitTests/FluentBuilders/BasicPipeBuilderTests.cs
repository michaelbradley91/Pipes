using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.FluentBuilders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class BasicPipeBuilderTests
    {
        private IBasicPipeBuilder basicPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            basicPipeBuilder = new BasicPipeBuilder();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithCapacity_GivenANegativeCapacity_ThrowsAnArgumentOutOfRangeException()
        {
            basicPipeBuilder.WithCapacity(-5);
        }

        [Test]
        public void WithNoCapacity_CreatesAPipeWithZeroCapacity()
        {
            // Act
            var pipe = basicPipeBuilder.WithNoCapacity();

            // Assert
            pipe.Capacity.Should().Be(0);
        }

        [Test]
        public void WithCapacity_GivenASpecificCapacity_CreatesAPipeWithThatCapacity()
        {
            // Act
            var pipe = basicPipeBuilder.WithCapacity(100);

            // Assert
            pipe.Capacity.Should().Be(100);
        }
    }
}
