using FluentAssertions;
using NUnit.Framework;
using Pipes.FluentBuilders;
using System;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class BasicPipeBuilderTests
    {
        private ISimplePipeBuilder<int> simplePipeBuilder;

        [SetUp]
        public void SetUp()
        {
            simplePipeBuilder = new SimplePipeBuilder<int>();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithCapacity_GivenANegativeCapacity_ThrowsAnArgumentOutOfRangeException()
        {
            simplePipeBuilder.WithCapacity(-5);
        }

        [Test]
        public void WithNoCapacity_CreatesAPipeWithZeroCapacity()
        {
            // Act
            var pipe = simplePipeBuilder.WithNoCapacity();

            // Assert
            pipe.Capacity.Should().Be(0);
        }

        [Test]
        public void WithCapacity_GivenASpecificCapacity_CreatesAPipeWithThatCapacity()
        {
            // Act
            var pipe = simplePipeBuilder.WithCapacity(100);

            // Assert
            pipe.Capacity.Should().Be(100);
        }
    }
}