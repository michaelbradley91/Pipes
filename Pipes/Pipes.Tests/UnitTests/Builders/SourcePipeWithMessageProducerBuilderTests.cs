using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SourcePipeWithMessageProducerBuilderTests
    {        
        [Test]
        public void TheMessageProducerPassedIntoTheConstructor_IsSetOnTheMessageProducerProperty()
        {
            // Arrange
            Func<int> messageProducer = () => 3;

            // Act
            var sourcePipeWithMessageProducerBuilder = new SourcePipeWithMessageProducerBuilder<int>(messageProducer);

            // Assert
            sourcePipeWithMessageProducerBuilder.Should().NotBeNull();
            sourcePipeWithMessageProducerBuilder.MessageProducer.Should().Be(messageProducer);
        }

        [Test]
        public void Build_GivenAMapInTheConstructor_ProducesATransformPipeWithThatMap()
        {
            // Arrange
            Func<int> messageProducer = () => 3;
            var sourcePipeWithMessageProducerBuilder = new SourcePipeWithMessageProducerBuilder<int>(messageProducer);

            // Act
            var pipe = sourcePipeWithMessageProducerBuilder.Build();

            // Assert
            pipe.MessageProducer.Should().Be(messageProducer);
            pipe.Outlet.Should().NotBeNull();
        }

        [Test]
        public void Build_WithTheMessageProducerSetInTheProperty_ProducesASourcePipeWithThatMessageProducer()
        {
            // Arrange
            Func<int> messageProducer = () => 3;
            var sourcePipeWithMessageProducerBuilder = new SourcePipeWithMessageProducerBuilder<int>(() => 1)
            {
                MessageProducer = messageProducer
            };
            // Act
            var pipe = sourcePipeWithMessageProducerBuilder.Build();

            // Assert
            pipe.MessageProducer.Should().Be(messageProducer);
            pipe.Outlet.Should().NotBeNull();
        }
    }
}