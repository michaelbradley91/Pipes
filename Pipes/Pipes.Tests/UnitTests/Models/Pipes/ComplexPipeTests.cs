using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

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
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResource.Create());

            outlet = new Mock<ISimpleOutlet<string>>();
            outlet.SetupGet(i => i.SharedResource).Returns(SharedResource.Create());

            complexPipe = new DummyPipe(inlet.Object, outlet.Object);
        }

        [Test]
        public void FindReceiver_GivenTheReceiverShouldBeNull_ReturnsNull()
        {
            // Arrange
            complexPipe.Receiver = null;

            // Act
            var receiver = complexPipe.FindReceiver(complexPipe.Inlet);

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
            var receiver = complexPipe.FindReceiver(complexPipe.Inlet);
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
            var sender = complexPipe.FindSender(complexPipe.Outlet);

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
            var sender = complexPipe.FindSender(complexPipe.Outlet);

            // Assert
            sender().Should().Be(message);
        }

        private class DummyPipe : ComplexPipe<int, string>
        {
            public ISimpleInlet<int> Inlet { get; }
            public ISimpleOutlet<string> Outlet { get; }  

            public DummyPipe(ISimpleInlet<int> inlet, ISimpleOutlet<string> outlet) 
                : base(new [] { inlet }, new [] { outlet })
            {
                Inlet = inlet;
                Outlet = outlet;
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
