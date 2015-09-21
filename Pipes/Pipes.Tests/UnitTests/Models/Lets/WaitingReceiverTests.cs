using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Lets;

namespace Pipes.Tests.UnitTests.Models.Lets
{
    [TestFixture]
    public class WaitingReceiverTests
    {
        [Test]
        public void WaitingReceiver_ShouldBeginWithTheMessageNotReceived()
        {
            // Act
            var waitingReceiver = new WaitingReceiver<int>();
            // Assert
            waitingReceiver.MessageReceived.Should().BeFalse();
        }

        [Test]
        public void WaitingReceiver_ShouldBeginWithASemaphoreWhichIsDown()
        {
            // Act
            var waitingReceiver = new WaitingReceiver<int>();

            // Assert (no excetion thrown)
            waitingReceiver.WaitSemaphore.Release();
        }

        [Test]
        public void ReceiveMessage_GivenAMessage_UpdatesTheMessageReceivedFlagToTrue()
        {
            // Arrange
            var waitingReceiver = new WaitingReceiver<int>();

            // Act
            waitingReceiver.ReceiveMessage(33);

            // Assert
            waitingReceiver.MessageReceived.Should().BeTrue();
        }

        [ExpectedException(typeof (InvalidOperationException))]
        [Test]
        public void ReceiveMessage_AfterAMessageHasAlreadyBeenReceived_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var waitingReceiver = new WaitingReceiver<int>();
            waitingReceiver.ReceiveMessage(3);

            // Act
            waitingReceiver.ReceiveMessage(5);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void GetMessage_BeforeAMessageHasBeenReceived_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var waitingReceiver = new WaitingReceiver<int>();

            // Act
            waitingReceiver.GetMessage();
        }

        [Test]
        public void GetMessage_AfterAMessageHasBeenReceived_ReturnsThatMessage()
        {
            // Arrange
            const int sentMessage = 13;

            var waitingReceiver = new WaitingReceiver<int>();
            waitingReceiver.ReceiveMessage(sentMessage);

            // Act
            var receivedMessage = waitingReceiver.GetMessage();

            // Assert
            receivedMessage.Should().Be(sentMessage);
        }
    }
}
