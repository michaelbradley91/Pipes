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
    public class EitherInletPipeTests
    {
        private Mock<ITwoWayTieBreaker> tieBreaker;
        private IEitherInletPipe<ITwoWayTieBreaker, int> eitherInletPipe;
        
        [SetUp]
        public void SetUp()
        {
            tieBreaker = new Mock<ITwoWayTieBreaker>();
            eitherInletPipe = PipeBuilder.New.EitherInletPipe<int>().WithTieBreaker(tieBreaker.Object).Build();
        }

        [Test]
        public void EitherInletPipe_HasTwoInlets()
        {
            // Assert
            eitherInletPipe.LeftInlet.Should().NotBeNull();
            eitherInletPipe.RightInlet.Should().NotBeNull();
            eitherInletPipe.ConnectableInlets.Count.Should().Be(2);
            eitherInletPipe.ConnectableInlets.Should().BeEquivalentTo(eitherInletPipe.LeftInlet, eitherInletPipe.RightInlet);
        }

        [Test]
        public void EitherInletPipe_HasOneOutlet()
        {
            // Assert
            eitherInletPipe.Outlet.Should().NotBeNull();
            eitherInletPipe.ConnectableOutlets.Count.Should().Be(1);
            eitherInletPipe.ConnectableOutlets.Single().Should().Be(eitherInletPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = eitherInletPipe.FindReceiver(eitherInletPipe.LeftInlet);

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
                receivedMessage = eitherInletPipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = eitherInletPipe.FindReceiver(eitherInletPipe.LeftInlet);

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
            var sender = eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnTheLeftInlet_ReturnsThatSender()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                eitherInletPipe.LeftInlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            sender.Should().NotBeNull();

            // Act
            var receivedMessage = sender();

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnTheRightInlet_ReturnsThatSender()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                eitherInletPipe.RightInlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = eitherInletPipe.FindSender(eitherInletPipe.Outlet);

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
            var mockInlet = (IInlet<int>)mockPipe.Object.ConnectableInlets.Single();

            eitherInletPipe.Outlet.ConnectTo(mockInlet);

            // Act
            eitherInletPipe.FindReceiver(eitherInletPipe.LeftInlet);

            // Assert
            mockPipe.Verify(p => p.FindReceiver(mockInlet));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsLeftInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.ConnectableOutlets.Single();

            eitherInletPipe.LeftInlet.ConnectTo(mockOutlet);

            // Act
            eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet));
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsRightInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            var mockOutlet = (IOutlet<int>)mockPipe.Object.ConnectableOutlets.Single();

            eitherInletPipe.RightInlet.ConnectTo(mockOutlet);

            // Act
            eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            mockPipe.Verify(p => p.FindSender(mockOutlet));
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnBothInletsAndTheTieBreakerResolvesToLeft_UsesTheLeftSender()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();

            var mockLeftPipeOutlet = (IOutlet<int>)mockLeftPipe.Object.ConnectableOutlets.Single();
            var mockRightPipeOutlet = (IOutlet<int>)mockRightPipe.Object.ConnectableOutlets.Single();

            eitherInletPipe.LeftInlet.ConnectTo(mockLeftPipeOutlet);
            eitherInletPipe.RightInlet.ConnectTo(mockRightPipeOutlet);

            const int message = 3;
            Func<int> leftSender = () => message;
            Func<int> rightSender = () => 4;
            mockLeftPipe.Setup(p => p.FindSender(mockLeftPipeOutlet)).Returns(leftSender);
            mockRightPipe.Setup(p => p.FindSender(mockRightPipeOutlet)).Returns(rightSender);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Left);

            // Act
            var sender = eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            sender().Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnBothInletsAndTheTieBreakerResolvesToRight_UsesTheRightSender()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();

            var mockLeftPipeOutlet = (IOutlet<int>)mockLeftPipe.Object.ConnectableOutlets.Single();
            var mockRightPipeOutlet = (IOutlet<int>)mockRightPipe.Object.ConnectableOutlets.Single();

            eitherInletPipe.LeftInlet.ConnectTo(mockLeftPipeOutlet);
            eitherInletPipe.RightInlet.ConnectTo(mockRightPipeOutlet);

            Func<int> leftSender = () => 3;
            const int message = 4;
            Func<int> rightSender = () => message;
            mockLeftPipe.Setup(p => p.FindSender(mockLeftPipeOutlet)).Returns(leftSender);
            mockRightPipe.Setup(p => p.FindSender(mockRightPipeOutlet)).Returns(rightSender);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Right);

            // Act
            var sender = eitherInletPipe.FindSender(eitherInletPipe.Outlet);

            // Assert
            sender().Should().Be(message);
        }
    }
}
