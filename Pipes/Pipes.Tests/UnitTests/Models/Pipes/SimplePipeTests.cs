using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class SimplePipeTests
    {
        private DummyPipe dummyPipe;

        [SetUp]
        public void SetUp()
        {
            dummyPipe = new DummyPipe();
        }

        [Test]
        public void FindReceiver_GivenTheReceiverShouldBeNull_ReturnsNull()
        {
            // Arrange
            dummyPipe.Receiver = null;

            // Act
            var receiver = dummyPipe.FindReceiver(dummyPipe.Inlet.Object, true);

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
            var receiver = dummyPipe.FindReceiver(dummyPipe.Inlet.Object, true);
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
            var sender = dummyPipe.FindSender(dummyPipe.Outlet.Object, true);

            // Assert
            sender.Should().BeNull();
        }

        public void FindSender_GivenTheSenderIsNotNull_WrapsTheSenderToCastItsTypes()
        {
            // Arrange
            const int message = 12;
            dummyPipe.Sender = () => message;

            // Act
            var sender = dummyPipe.FindSender(dummyPipe.Outlet.Object, true);

            // Assert
            sender().Should().Be(message);
        }

        private class DummyPipe : SimplePipe<int>
        {
            public readonly Mock<IInlet<int>> Inlet = new Mock<IInlet<int>>();
            public readonly Mock<IOutlet<int>> Outlet = new Mock<IOutlet<int>>();

            public override IReadOnlyCollection<IInlet> AllInlets => new List<IInlet<int>> {Inlet.Object};
            public override IReadOnlyCollection<IOutlet> AllOutlets => new List<IOutlet<int>> { Outlet.Object };

            public override SharedResource SharedResource => null;

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
