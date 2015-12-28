using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
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
            basicPipe.AllInlets.Count.Should().Be(1);
            basicPipe.AllInlets.Single().Should().Be(basicPipe.Inlet);
        }

        [Test]
        public void BasicPipe_HasOneOutlet()
        {
            // Assert
            basicPipe.Outlet.Should().NotBeNull();
            basicPipe.AllOutlets.Count.Should().Be(1);
            basicPipe.AllOutlets.Single().Should().Be(basicPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = basicPipe.FindReceiver(basicPipe.Inlet);

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
            var receiver = basicPipe.FindReceiver(basicPipe.Inlet);

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
            var sender = basicPipe.FindSender(basicPipe.Outlet);

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
            var sender = basicPipe.FindSender(basicPipe.Outlet);

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
            var mockInlet = (IInlet<int>)mockPipe.Object.AllInlets.Single();

            basicPipe.Outlet.ConnectTo(mockInlet);

            // Act
            basicPipe.FindReceiver(basicPipe.Inlet);
            

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet, true));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.AllOutlets.Single();

            basicPipe.Inlet.ConnectTo(mockOutlet);

            // Act
            basicPipe.FindSender(basicPipe.Outlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet, true));
        }
    }
}
