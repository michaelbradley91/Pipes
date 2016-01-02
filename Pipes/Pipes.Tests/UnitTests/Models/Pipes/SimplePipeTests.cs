using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class SimplePipeTests
    {
        private Mock<ISimpleInlet<int>> inlet;
        private Mock<ISimpleOutlet<int>> outlet;
        private DummyPipe dummyPipe;

        [SetUp]
        public void SetUp()
        {
            inlet = new Mock<ISimpleInlet<int>>();
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            outlet = new Mock<ISimpleOutlet<int>>();
            outlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            dummyPipe = new DummyPipe(inlet.Object, outlet.Object);
        }

        [Test]
        public void FindReceiver_GivenTheReceiverShouldBeNull_ReturnsNull()
        {
            // Arrange
            dummyPipe.Receiver = null;

            // Act
            var receiver = dummyPipe.FindReceiver(inlet.Object);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenTheReceiverIsNotNull_WrapsTheReceiverToCastItsTypes()
        {
            // Arrange
            var messageReceived = 0;
            dummyPipe.Receiver = m => messageReceived = m;

            // Act
            var receiver = dummyPipe.FindReceiver(inlet.Object);
            const int message = 223;
            receiver(message);

            // Assert
            messageReceived.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenTheSenderShouldBeNull_ReturnsNull()
        {
            // Arrange
            dummyPipe.Sender = null;

            // Act
            var sender = dummyPipe.FindSender(outlet.Object);

            // Assert
            sender.Should().BeNull();
        }

        public void FindSender_GivenTheSenderIsNotNull_WrapsTheSenderToCastItsTypes()
        {
            // Arrange
            const int message = 12;
            dummyPipe.Sender = () => message;

            // Act
            var sender = dummyPipe.FindSender(outlet.Object);

            // Assert
            sender().Should().Be(message);
        }

        private class DummyPipe : SimplePipe<int>
        {
            public DummyPipe(IInlet<int> inlet, IOutlet<int> outlet)
                : base(new[] {inlet}, new[] {outlet})
            {
            }
            
            public Action<int> Receiver = i => { };
            public Func<int> Sender = () => 3;

            protected override Action<int> FindReceiver(IInlet<int> inletSendingMessage)
            {
                return Receiver;
            }

            protected override Func<int> FindSender(IOutlet<int> outletReceivingMessage)
            {
                return Sender;
            }
        }
    }
}
