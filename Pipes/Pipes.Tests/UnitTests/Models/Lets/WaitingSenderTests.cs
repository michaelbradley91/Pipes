using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Lets;

namespace Pipes.Tests.UnitTests.Models.Lets
{
    [TestFixture]
    public class WaitingSenderTests
    {
        [Test]
        public void WaitingSender_ShouldBeginWithTheMessageNotMarkedAsSent()
        {
            // Act
            var waitingSender = new WaitingSender<int>(3);

            // Assert
            waitingSender.MessageSent.Should().BeFalse();
        }

        [Test]
        public void WaitingSender_ShouldBeginWithASemaphoreDown()
        {
            // Act
            var waitingSender = new WaitingSender<int>(3);

            // Assert (no exception is thrown)
            waitingSender.WaitSemaphore.Release();
        }

        [Test]
        public void WaitingSender_GivenAMessage_ReturnsThatMessageWhenAskedForIt()
        {
            // Arrange
            const int sentMessage = 4;
            var waitingSender = new WaitingSender<int>(sentMessage);

            // Act
            var receivedMessage = waitingSender.Message;

            // Assert
            receivedMessage.Should().Be(sentMessage);
        }

        [Test]
        public void RecordMessageSent_GivenTheMessageHasNotBeenRecordAsSent_UpdatesTheMessageSentFlagToTrue()
        {
            // Arrange
            var waitingSender = new WaitingSender<int>(4);

            // Act
            waitingSender.RecordMessageSent();

            // Assert
            waitingSender.MessageSent.Should().BeTrue();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void RecordMessageSent_GivenTheMessageHasAlreadyBeenRecordedAsSent_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var waitingSender = new WaitingSender<int>(3);
            waitingSender.RecordMessageSent();

            // Act
            waitingSender.RecordMessageSent();
        }
    }
}
