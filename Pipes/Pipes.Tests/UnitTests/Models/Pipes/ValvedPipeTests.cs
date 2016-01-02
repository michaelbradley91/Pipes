using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Constants;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class ValvedPipeTests
    {
        private Mock<ISimpleInlet<int>> inlet;
        private Mock<ISimpleOutlet<string>> outlet;
        private Mock<ITieBreaker> tieBreaker;

        private ValvedPipe<int, string, ITieBreaker> valvedPipe;

        [SetUp]
        public void SetUp()
        {
            inlet = new Mock<ISimpleInlet<int>>();
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());
            inlet.SetupGet(i => i.Pipe).Returns(() => valvedPipe);

            outlet = new Mock<ISimpleOutlet<string>>();
            outlet.SetupGet(o => o.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());
            outlet.SetupGet(i => i.Pipe).Returns(() => valvedPipe);

            tieBreaker = new Mock<ITieBreaker>();

            valvedPipe = new ValvedPipe<int, string, ITieBreaker>(inlet.Object, outlet.Object, tieBreaker.Object);
        }

        [Test]
        public void ValvedPipe_HasOneInlet()
        {
            valvedPipe.AllInlets.Should().HaveCount(1);
            valvedPipe.Inlet.Should().NotBeNull();
            valvedPipe.Inlet.Should().Be(valvedPipe.AllInlets.Single());
        }

        [Test]
        public void ValvedPipe_HasOneOutlet()
        {
            valvedPipe.AllOutlets.Should().HaveCount(1);
            valvedPipe.Outlet.Should().NotBeNull();
            valvedPipe.Outlet.Should().Be(valvedPipe.AllOutlets.Single());
        }

        [Test]
        public void ValvedPipe_HasOneValve()
        {
            valvedPipe.Valve.Should().NotBeNull();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindSender_GivenAnOutletThatDoesNotBelongToThePipe_ThrowsAnException()
        {
            // Arrange
            var dummyOutlet = new Mock<IOutlet<int>>().Object;

            // Act
            valvedPipe.FindSender(dummyOutlet);
        }

        [Test]
        public void FindSender_GivenAnOutletThatDoesNotBelongToThePipeButToldNotToCheckIt_ReturnsNull()
        {
            // Arrange
            var dummyOutlet = new Mock<IOutlet<int>>().Object;

            // Act
            var sender = valvedPipe.FindSender(dummyOutlet, false);

            // Assert
            sender.Should().BeNull();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindReceiver_GivenAnInletThatDoesNotBelongToThePipe_ThrowsAnException()
        {
            // Arrange
            var dummyInlet = new Mock<IInlet<int>>().Object;

            // Act
            valvedPipe.FindReceiver(dummyInlet);
        }

        [Test]
        public void FindReceiver_GivenAnInletThatDoesNotBelongToThePipeButToldNotToCheckIt_ReturnsNull()
        {
            // Arrange
            var dummyInlet = new Mock<IInlet<int>>().Object;

            // Act
            var receiver = valvedPipe.FindReceiver(dummyInlet, false);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsNeitherASenderNorAReceiver_BlocksIndefinitely()
        {
            // Act
            ThreadInterruptedException exception = null;
            var thread = ThreadHelpers.RunInThread(() =>
            {
                try
                {
                    valvedPipe.Valve.ReceiveOrSend("Hello");
                }
                catch (ThreadInterruptedException e)
                {
                    exception = e;
                }
            });
            Thread.Sleep(1000);
            thread.Interrupt();
            Thread.Sleep(500);

            // Assert
            exception.Should().NotBeNull();
        }

        [ExpectedException(typeof(TimeoutException))]
        [Test]
        public void ReceiveOrSendWithATimeout_GivenThereIsNeitherASenderNorAReceiver_BlocksUntilTheTimeoutExpiresAndThrowsTheException()
        {
            valvedPipe.Valve.ReceiveOrSend("Hello", TimeSpan.FromMilliseconds(1000));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveOrSendImmediately_GivenThereIsNeitherASenderNorAReceiver_ThrowsAnExceptionImmediately()
        {
            valvedPipe.Valve.ReceiveOrSendImmediately("Hello");
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsAReceiverEventually_ReturnsTheMessageWasSent()
        {
            // Arrange
            const string message = "This is confusing...";

            var receivedMessage = "";
            ThreadHelpers.RunInThread(() =>
            {
                Thread.Sleep(500);
                var sender = valvedPipe.FindSender(valvedPipe.Outlet);
                receivedMessage = sender();
            });

            // Act
            var result = valvedPipe.Valve.ReceiveOrSend(message);
            Thread.Sleep(500);

            // Assert
            result.MessageSent.Should().BeTrue();
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsAlreadyAReceiver_ReturnsTheMessageWasSent()
        {
            // Arrange
            const string message = "This is sooo confusing...";

            var receivedMessage = "";
            outlet.Setup(o => o.FindReceiver()).Returns(m => receivedMessage = m);

            // Act
            var result = valvedPipe.Valve.ReceiveOrSend(message);

            // Assert
            result.MessageSent.Should().BeTrue();
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsASenderEventually_ReturnsAReceivedMessage()
        {
            // Arrange
            const int message = 118722;

            ThreadHelpers.RunInThread(() =>
            {
                Thread.Sleep(500);
                var receiver = valvedPipe.FindReceiver(valvedPipe.Inlet);
                receiver(message);
            });

            // Act
            var result = valvedPipe.Valve.ReceiveOrSend("Not getting sent");
            Thread.Sleep(500);

            // Assert
            result.MessageReceived.Should().BeTrue();
            result.GetReceivedMessage().Should().Be(message);
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsAlreadyASender_ReturnsAReceivedMessage()
        {
            // Arrange
            const int message = 121221;

            inlet.Setup(o => o.FindSender()).Returns(() => message);

            // Act
            var result = valvedPipe.Valve.ReceiveOrSend("Not getting sent...");

            // Assert
            result.MessageReceived.Should().BeTrue();
            result.GetReceivedMessage().Should().Be(message);
        }

        [Test]
        public void ReceiveOrSend_GivenThereIsASenderAndAReceiverAlready_UsesTheTieBreaker()
        {
            // Arrange
            const int message = 121221;

            inlet.Setup(o => o.FindSender()).Returns(() => message);
            outlet.Setup(o => o.FindReceiver()).Returns(m => { });
            // Right means the message should be received.
            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Right);

            // Act
            var result = valvedPipe.Valve.ReceiveOrSend("Not getting sent...");

            // Assert
            result.MessageReceived.Should().BeTrue();
            result.GetReceivedMessage().Should().Be(message);
            tieBreaker.Verify(t => t.ResolveTie(), Times.Once);
        }
    }
}
