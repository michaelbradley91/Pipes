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
    public class TransformPipeTests
    {
        private ITransformPipe<int, string> transformPipe;

        [SetUp]
        public void SetUp()
        {
            transformPipe = PipeBuilder.New.TransformPipe<int, string>().WithMap(i => i.ToString()).Build();
        }

        [Test]
        public void TransformPipe_IsAComplexPipe()
        {
            // Assert
            transformPipe.Should().BeAssignableTo<ComplexPipe<int, string>>();
        }
        
        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = transformPipe.FindReceiver(transformPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiver_ReturnsAReceiverThatAppliesTheMap()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(string);
            var thread = new Thread(() =>
            {
                receivedMessage = transformPipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = transformPipe.FindReceiver(transformPipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message.ToString());
        }

        [Test]
        public void FindSender_GivenThereIsNoSender_ReturnsNull()
        {
            // Act
            var sender = transformPipe.FindSender(transformPipe.Outlet);

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenThereIsASender_ReturnsASenderThatAppliesTheMap()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                transformPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = transformPipe.FindSender(transformPipe.Outlet);

            // Assert
            sender.Should().NotBeNull();

            // Act
            var receivedMessage = sender();

            // Assert
            receivedMessage.Should().Be(message.ToString());
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<string>();
            var mockInlet = (IInlet<string>)mockPipe.Object.ConnectableInlets.Single();

            transformPipe.Outlet.ConnectTo(mockInlet);

            // Act
            transformPipe.FindReceiver(transformPipe.Inlet);
            

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.ConnectableOutlets.Single();

            transformPipe.Inlet.ConnectTo(mockOutlet);

            // Act
            transformPipe.FindSender(transformPipe.Outlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet));
        }
    }
}
