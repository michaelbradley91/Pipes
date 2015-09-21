using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class BasicPipeTests
    {
        private IBasicPipe<int> basicPipe;

        [SetUp]
        public void SetUp()
        {
            basicPipe = PipeBuilder.New.BasicPipe<int>().Build();
        }

        [Test]
        public void BasicPipe_HasOneInlet()
        {
            // Assert
            basicPipe.Inlet.Should().NotBeNull();
            basicPipe.Inlets.Count.Should().Be(1);
            basicPipe.Inlets.Single().Should().Be(basicPipe.Inlet);
        }

        [Test]
        public void BasicPipe_HasOneOutlet()
        {
            // Assert
            basicPipe.Outlet.Should().NotBeNull();
            basicPipe.Outlets.Count.Should().Be(1);
            basicPipe.Outlets.Single().Should().Be(basicPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = basicPipe.FindReceiver();

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiver_ReturnsThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = basicPipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = basicPipe.FindReceiver();

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThereIsNoSender_ReturnsNull()
        {
            // Act
            var sender = basicPipe.FindSender();

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenThereIsASender_ReturnsThatSender()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                basicPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = basicPipe.FindSender();

            // Assert
            sender.Should().NotBeNull();

            // Act
            var receivedMessage = sender();

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            basicPipe.Outlet.ConnectTo(mockPipe.Object.Inlets.Single());

            // Act
            basicPipe.FindReceiver();

            // Assert
            mockPipe.Verify(p => p.FindReceiver());
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            basicPipe.Inlet.ConnectTo(mockPipe.Object.Outlets.Single());

            // Act
            basicPipe.FindSender();

            // Assert
            mockPipe.Verify(p => p.FindSender());
        }
    }
}
