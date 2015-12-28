using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SourcePipeBuilderTests
    {
        private ISourcePipeBuilder<int> sourcePipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sourcePipeBuilder = new SourcePipeBuilder<int>();
        }

        [Test]
        public void WithMessageProducer_ReturnsASourcePipeWithMessageProducerBuilder_WithTheSpecifiedMessageProducer()
        {
            // Arrange
            Func<int> messageProducer = () => 3;

            // Act
            var sourcePipeWithMessageProducerBuilder = sourcePipeBuilder.WithMessageProducer(messageProducer);

            // Assert
            sourcePipeWithMessageProducerBuilder.Should().NotBeNull();
            sourcePipeWithMessageProducerBuilder.MessageProducer.Should().Be(messageProducer);
        }
    }
}