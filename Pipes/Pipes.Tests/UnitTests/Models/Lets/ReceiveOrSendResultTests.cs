using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Lets;

namespace Pipes.Tests.UnitTests.Models.Lets
{
    [TestFixture]
    public class ReceiveOrSendResultTests
    {
        [Test]
        public void CreateSendResult_ReturnsAResult_WithMessageSentTrue()
        {
            // Arrange
            var result = ReceiveOrSendResult<int, string>.CreateSendResult();

            // Act
            var messageSent = result.MessageSent;

            // Assert
            messageSent.Should().BeTrue();
        }

        [Test]
        public void CreateSendResult_ReturnsAResult_WithMessageReceivedFalse()
        {
            // Arrange
            var result = ReceiveOrSendResult<int, string>.CreateSendResult();

            // Act
            var messageReceived = result.MessageReceived;

            // Assert
            messageReceived.Should().BeFalse();
        }

        [Test]
        public void CreateReceiveResult_ReturnsAResult_WithMessageSentFalse()
        {
            // Arrange
            var result = ReceiveOrSendResult<int, string>.CreateReceiveResult(3);

            // Act
            var messageSent = result.MessageSent;

            // Assert
            messageSent.Should().BeFalse();
        }

        [Test]
        public void CreateReceiveResult_ReturnsAResult_WithMessageReceivedTrue()
        {
            // Arrange
            var result = ReceiveOrSendResult<int, string>.CreateReceiveResult(3);

            // Act
            var messageSent = result.MessageReceived;

            // Assert
            messageSent.Should().BeTrue();
        }

        [Test]
        public void CreateReceiveResult_ReturnsAResult_WithTheMessageReceived()
        {
            // Arrange
            const int expectedMessage = 3;
            var result = ReceiveOrSendResult<int, string>.CreateReceiveResult(expectedMessage);

            // Act
            var actualMessage = result.GetReceivedMessage();

            // Assert
            actualMessage.Should().Be(expectedMessage);
        }
    }
}
