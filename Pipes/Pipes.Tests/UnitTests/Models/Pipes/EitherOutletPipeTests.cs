using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class EitherOutletPipeTests
    {
        private Mock<ITwoWayTieBreaker> tieBreaker;
        private IEitherOutletPipe<ITwoWayTieBreaker, int> eitherOutletPipe;
        
        [SetUp]
        public void SetUp()
        {
            tieBreaker = new Mock<ITwoWayTieBreaker>();
            eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().WithTieBreaker(tieBreaker.Object).Build();
        }

        [Test]
        public void EitherOutletPipe_HasOneInlet()
        {
            // Assert
            eitherOutletPipe.Inlet.Should().NotBeNull();
            eitherOutletPipe.ConnectableInlets.Count.Should().Be(1);
            eitherOutletPipe.ConnectableInlets.Single().Should().Be(eitherOutletPipe.Inlet);
        }

        [Test]
        public void EitherOutletPipe_HasTwoOutlets()
        {
            // Assert
            eitherOutletPipe.LeftOutlet.Should().NotBeNull();
            eitherOutletPipe.RightOutlet.Should().NotBeNull();
            eitherOutletPipe.ConnectableOutlets.Count.Should().Be(2);
            eitherOutletPipe.ConnectableOutlets.Should().BeEquivalentTo(eitherOutletPipe.LeftOutlet, eitherOutletPipe.RightOutlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnTheLeftOutlet_ReturnsThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = eitherOutletPipe.LeftOutlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnTheRightOutlet_ReturnsThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = eitherOutletPipe.RightOutlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);

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
            var sender = eitherOutletPipe.FindSender(eitherOutletPipe.LeftOutlet);

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
                eitherOutletPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = eitherOutletPipe.FindSender(eitherOutletPipe.LeftOutlet);

            // Assert
            sender.Should().NotBeNull();

            // Act
            var receivedMessage = sender();

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsLeftOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockInlet = (IInlet<int>)mockPipe.Object.ConnectableInlets.Single();

            eitherOutletPipe.LeftOutlet.ConnectTo(mockInlet);

            // Act
            eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet));
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsRightOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockInlet = (IInlet<int>)mockPipe.Object.ConnectableInlets.Single();

            eitherOutletPipe.RightOutlet.ConnectTo(mockInlet);

            // Act
            eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.ConnectableOutlets.Single();

            eitherOutletPipe.Inlet.ConnectTo(mockOutlet);

            // Act
            eitherOutletPipe.FindSender(eitherOutletPipe.LeftOutlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet));
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnBothOutletsAndTheTieBreakerResolvesToLeft_UsesTheLeftReceiver()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();

            var mockLeftPipeInlet = (IInlet<int>)mockLeftPipe.Object.ConnectableInlets.Single();
            var mockRightPipeInlet = (IInlet<int>)mockRightPipe.Object.ConnectableInlets.Single();

            eitherOutletPipe.LeftOutlet.ConnectTo(mockLeftPipeInlet);
            eitherOutletPipe.RightOutlet.ConnectTo(mockRightPipeInlet);

            var receivedMessage = 0;
            Action<int> leftReceiver = m => { receivedMessage = m; };
            Action<int> rightReceiver = m => { };
            mockLeftPipe.Setup(p => p.FindReceiver(mockLeftPipeInlet)).Returns(leftReceiver);
            mockRightPipe.Setup(p => p.FindReceiver(mockRightPipeInlet)).Returns(rightReceiver);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Left);

            // Act
            var sender = eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);
            const int message = 12;
            sender(message);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnBothOutletsAndTheTieBreakerResolvesToRight_UsesTheRightReceiver()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();

            var mockLeftPipeInlet = (IInlet<int>)mockLeftPipe.Object.ConnectableInlets.Single();
            var mockRightPipeInlet = (IInlet<int>)mockRightPipe.Object.ConnectableInlets.Single();

            eitherOutletPipe.LeftOutlet.ConnectTo(mockLeftPipeInlet);
            eitherOutletPipe.RightOutlet.ConnectTo(mockRightPipeInlet);

            Action<int> leftReceiver = m => { };
            var receivedMessage = 0;
            Action<int> rightReceiver = m => { receivedMessage = m; };
            mockLeftPipe.Setup(p => p.FindReceiver(mockLeftPipeInlet)).Returns(leftReceiver);
            mockRightPipe.Setup(p => p.FindReceiver(mockRightPipeInlet)).Returns(rightReceiver);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Right);

            // Act
            var sender = eitherOutletPipe.FindReceiver(eitherOutletPipe.Inlet);
            const int message = 1313;
            sender(message);

            // Assert
            receivedMessage.Should().Be(message);
        }
    }
}
