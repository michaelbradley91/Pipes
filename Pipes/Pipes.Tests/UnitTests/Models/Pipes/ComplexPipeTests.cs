using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class ComplexPipeTests
    {
        private DummyPipe complexPipe;
        private Mock<ISimpleInlet<int>> inlet;
        private Mock<ISimpleOutlet<string>> outlet;

        [SetUp]
        public void SetUp()
        {
            inlet = new Mock<ISimpleInlet<int>>();
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            outlet = new Mock<ISimpleOutlet<string>>();
            outlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            complexPipe = new DummyPipe(inlet.Object, outlet.Object);
        }

        [Test]
        public void ComplexPipe_HasOneInlet()
        {
            // Assert
            complexPipe.Inlet.Should().NotBeNull();
            complexPipe.AllInlets.Count.Should().Be(1);
            complexPipe.AllInlets.Single().Should().Be(complexPipe.Inlet);
        }

        [Test]
        public void ComplexPipe_HasOneOutlet()
        {
            // Assert
            complexPipe.Outlet.Should().NotBeNull();
            complexPipe.AllOutlets.Count.Should().Be(1);
            complexPipe.AllOutlets.Single().Should().Be(complexPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenTheReceiverShouldBeNull_ReturnsNull()
        {
            // Arrange
            complexPipe.Receiver = null;

            // Act
            var receiver = complexPipe.FindReceiver(complexPipe.Inlet, true);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenTheReceiverIsNotNull_WrapsTheReceiverToCastItsTypes()
        {
            // Arrange
            var messageReceived = 0;
            complexPipe.Receiver = m => messageReceived = m;

            // Act
            var receiver = complexPipe.FindReceiver(complexPipe.Inlet, true);
            const int message = 223;
            receiver(message);

            // Assert
            messageReceived.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenTheSenderShouldBeNull_ReturnsNull()
        {
            // Arrange
            complexPipe.Sender = null;

            // Act
            var sender = complexPipe.FindSender(complexPipe.Outlet, true);

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenTheSenderIsNotNull_WrapsTheSenderToCastItsTypes()
        {
            // Arrange
            const string message = "12";
            complexPipe.Sender = () => message;

            // Act
            var sender = complexPipe.FindSender(complexPipe.Outlet, true);

            // Assert
            sender().Should().Be(message);
        }

        private class DummyPipe : ComplexPipe<int, string>
        {
            public DummyPipe(ISimpleInlet<int> inlet, ISimpleOutlet<string> outlet) : base(inlet, outlet)
            {
            }

            public Action<int> Receiver = i => { };
            public Func<string> Sender = () => "3";

            protected override Action<int> FindReceiver(IInlet<int> inletSendingMessage)
            {
                return Receiver;
            }

            protected override Func<string> FindSender(IOutlet<string> outletReceivingMessage)
            {
                return Sender;
            }
        }
    }
}
