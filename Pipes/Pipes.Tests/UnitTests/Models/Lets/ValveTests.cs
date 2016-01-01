using System;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;

namespace Pipes.Tests.UnitTests.Models.Lets
{
    [TestFixture]
    public class ValveTests
    {
        private Mock<ISimpleInlet<string>> preparationInlet;
        private Mock<ISimpleOutlet<string>> flushOutlet;
        private Mock<ISimpleOutlet<ReceiveOrSendResult<int, string>>> resultOutlet;

        private Valve<int, string> valve;

        [SetUp]
        public void SetUp()
        {
            preparationInlet = new Mock<ISimpleInlet<string>>();
            flushOutlet = new Mock<ISimpleOutlet<string>>();
            resultOutlet = new Mock<ISimpleOutlet<ReceiveOrSendResult<int, string>>>();
            resultOutlet.Setup(r => r.Receive()).Returns(ReceiveOrSendResult<int, string>.CreateSendResult);
            resultOutlet.Setup(r => r.ReceiveImmediately()).Returns(ReceiveOrSendResult<int, string>.CreateSendResult);
            resultOutlet.Setup(r => r.Receive(It.IsAny<TimeSpan>())).Returns(ReceiveOrSendResult<int, string>.CreateSendResult);

            valve = new Valve<int, string>(preparationInlet.Object, flushOutlet.Object, resultOutlet.Object);
        }

        [Test]
        public void ReceiveOrSend_FirstPreparesTheMessageToBeSent()
        {
            // Arrange
            const string message = "Hello";

            // Act
            valve.ReceiveOrSend(message);

            // Assert
            preparationInlet.Verify(p => p.Send(message), Times.Once);
        }

        [Test]
        public void ReceiveOrSend_GivenASendResultIsReturned_ReturnsThatResult()
        {
            // Arrange
            var expectedResult = ReceiveOrSendResult<int, string>.CreateSendResult();
            resultOutlet.Setup(r => r.Receive()).Returns(expectedResult);

            // Act
            var actualResult = valve.ReceiveOrSend("hello");

            // Assert
            actualResult.MessageSent.Should().BeTrue();
        }

        [Test]
        public void ReceiveOrSend_GivenAReceiveResultIsReturned_ReturnsThatResult()
        {
            // Arrange
            const int message = 3;
            var expectedResult = ReceiveOrSendResult<int, string>.CreateReceiveResult(message);
            resultOutlet.Setup(r => r.Receive()).Returns(expectedResult);

            // Act
            var actualResult = valve.ReceiveOrSend("Hello");

            // Assert
            actualResult.MessageReceived.Should().BeTrue();
            actualResult.GetReceivedMessage().Should().Be(message);
        }

        [Test]
        public void ReceiveOrSend_GivenAReceiveResult_FlushesTheMessageToSend()
        {
            // Arrange
            resultOutlet.Setup(r => r.Receive()).Returns(ReceiveOrSendResult<int, string>.CreateReceiveResult(3));

            // Act
            valve.ReceiveOrSend("Wooo");

            // Assert
            flushOutlet.Verify(f => f.ReceiveImmediately(), Times.Once);
        }

        [Test]
        public void ReceiveOrSend_GivenTheThreadIsAbortedBeforePreparationCompletes_JustThrowsTheException()
        {
            // Arrange
            preparationInlet.Setup(p => p.Send(It.IsAny<string>())).Throws<ThreadInterruptedException>();

            // Act
            ThreadInterruptedException exception = null;
            try
            {
                valve.ReceiveOrSend("Not gonna make it...");
            }
            catch (ThreadInterruptedException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.Send(It.IsAny<string>()), Times.Once);
            resultOutlet.Verify(r => r.Receive(), Times.Never);
            exception.Should().NotBeNull();
        }

        [Test]
        public void ReceiveOrSend_GivenTheThreadIsAbortedDuringReceivingAResult_FlushesTheMessageToBeSentAndThrowsTheException()
        {
            // Arrange
            resultOutlet.Setup(r => r.Receive()).Throws<ThreadInterruptedException>();

            // Act
            ThreadInterruptedException exception = null;
            try
            {
                valve.ReceiveOrSend("Not gonna make it...");
            }
            catch (ThreadInterruptedException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.Send(It.IsAny<string>()), Times.Once);
            resultOutlet.Verify(r => r.Receive(), Times.Once);
            flushOutlet.Verify(f => f.ReceiveImmediately(), Times.Once);
            exception.Should().NotBeNull();
        }

        [Test]
        public void ReceiveOrSend_GivenTheTimeoutExpiresBeforePreparationCompletes_JustThrowsTheException()
        {
            // Arrange
            preparationInlet.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<TimeSpan>())).Throws<TimeoutException>();

            // Act
            TimeoutException exception = null;
            try
            {
                valve.ReceiveOrSend("Not gonna make it...", TimeSpan.FromMilliseconds(1000));
            }
            catch (TimeoutException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.Send(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
            resultOutlet.Verify(r => r.Receive(It.IsAny<TimeSpan>()), Times.Never);
            exception.Should().NotBeNull();
        }

        [Test]
        public void ReceiveOrSend_GivenTheTimeoutExpiresDuringReceivingAResult_FlushesTheMessageToBeSentAndThrowsTheException()
        {
            // Arrange
            resultOutlet.Setup(r => r.Receive(It.IsAny<TimeSpan>())).Throws<TimeoutException>();

            // Act
            TimeoutException exception = null;
            try
            {
                valve.ReceiveOrSend("Not gonna make it...", TimeSpan.FromMilliseconds(1000));
            }
            catch (TimeoutException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.Send(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
            resultOutlet.Verify(r => r.Receive(It.IsAny<TimeSpan>()), Times.Once);
            flushOutlet.Verify(f => f.ReceiveImmediately(), Times.Once);
            exception.Should().NotBeNull();
        }

        [Test]
        public void ReceiveOrSend_GivenATimeout_PassesTheTimeoutMinusThePreparationTimeToTheResultOutlet()
        {
            // Arrange
            preparationInlet.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<TimeSpan>())).Callback((string s, TimeSpan t) => Thread.Sleep(1000));

            // Act
            valve.ReceiveOrSend("Long time...", TimeSpan.FromMilliseconds(2000));

            // Assert
            resultOutlet.Verify(r => r.Receive(It.Is<TimeSpan>(t => t < TimeSpan.FromMilliseconds(1500))), Times.Once);
        }

        [Test]
        public void ReceiveOrSendImmediately_GivenPreparationIsNotImmediate_JustThrowsTheException()
        {
            // Arrange
            preparationInlet.Setup(p => p.SendImmediately(It.IsAny<string>())).Throws<InvalidOperationException>();

            // Act
            InvalidOperationException exception = null;
            try
            {
                valve.ReceiveOrSendImmediately("Not gonna make it...");
            }
            catch (InvalidOperationException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.SendImmediately(It.IsAny<string>()), Times.Once);
            resultOutlet.Verify(r => r.ReceiveImmediately(), Times.Never);
            exception.Should().NotBeNull();
        }

        [Test]
        public void ReceiveOrSendImmediately_GivenReceivingAResultIsNotImmediate_FlushesTheMessageToBeSentAndThrowsTheException()
        {
            // Arrange
            resultOutlet.Setup(r => r.ReceiveImmediately()).Throws<InvalidOperationException>();

            // Act
            InvalidOperationException exception = null;
            try
            {
                valve.ReceiveOrSendImmediately("Not gonna make it...");
            }
            catch (InvalidOperationException e)
            {
                exception = e;
            }

            // Assert
            preparationInlet.Verify(p => p.SendImmediately(It.IsAny<string>()), Times.Once);
            resultOutlet.Verify(r => r.ReceiveImmediately(), Times.Once);
            flushOutlet.Verify(f => f.ReceiveImmediately(), Times.Once);
            exception.Should().NotBeNull();
        }
    }
}
