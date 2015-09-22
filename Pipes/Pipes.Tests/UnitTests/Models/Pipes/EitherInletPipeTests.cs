using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Constants;
using Pipes.Models.Pipes;
using Pipes.Models.TieBreakers;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class EitherInletPipeTests
    {
        private Mock<ITieBreaker> tieBreaker;
        private IEitherInletPipe<ITieBreaker, int> eitherInletPipe;
        
        [SetUp]
        public void SetUp()
        {
            tieBreaker = new Mock<ITieBreaker>();
            eitherInletPipe = PipeBuilder.New.EitherInletPipe<int>().WithTieBreaker(tieBreaker.Object).Build();
        }

        [Test]
        public void EitherInletPipe_HasTwoInlets()
        {
            // Assert
            eitherInletPipe.LeftInlet.Should().NotBeNull();
            eitherInletPipe.RightInlet.Should().NotBeNull();
            eitherInletPipe.Inlets.Count.Should().Be(2);
            eitherInletPipe.Inlets.Should().BeEquivalentTo(eitherInletPipe.LeftInlet, eitherInletPipe.RightInlet);
        }

        [Test]
        public void EitherInletPipe_HasOneOutlet()
        {
            // Assert
            eitherInletPipe.Outlet.Should().NotBeNull();
            eitherInletPipe.Outlets.Count.Should().Be(1);
            eitherInletPipe.Outlets.Single().Should().Be(eitherInletPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = eitherInletPipe.FindReceiver();

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
            var receiver = eitherInletPipe.FindReceiver();

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
            var sender = eitherInletPipe.FindSender();

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
            var sender = eitherInletPipe.FindSender();

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
            var sender = eitherInletPipe.FindSender();

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
            eitherInletPipe.Outlet.ConnectTo(mockPipe.Object.Inlets.Single());

            // Act
            eitherInletPipe.FindReceiver();

            // Assert
            mockPipe.Verify(p => p.FindReceiver());
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsLeftInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            eitherInletPipe.LeftInlet.ConnectTo(mockPipe.Object.Outlets.Single());

            // Act
            eitherInletPipe.FindSender();

            // Assert
            mockPipe.Verify(p => p.FindSender());
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsRightInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            eitherInletPipe.RightInlet.ConnectTo(mockPipe.Object.Outlets.Single());

            // Act
            eitherInletPipe.FindSender();

            // Assert
            mockPipe.Verify(p => p.FindSender());
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnBothInletsAndTheTieBreakerResolvesToLeft_UsesTheLeftSender()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();
            eitherInletPipe.LeftInlet.ConnectTo(mockLeftPipe.Object.Outlets.Single());
            eitherInletPipe.RightInlet.ConnectTo(mockRightPipe.Object.Outlets.Single());

            Func<int> leftSender = () => 3;
            Func<int> rightSender = () => 4;
            mockLeftPipe.Setup(p => p.FindSender()).Returns(leftSender);
            mockRightPipe.Setup(p => p.FindSender()).Returns(rightSender);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Left);

            // Act
            var sender = eitherInletPipe.FindSender();

            // Assert
            sender.Should().Be(leftSender);
        }

        [Test]
        public void FindSender_GivenThereIsASenderOnBothInletsAndTheTieBreakerResolvesToRight_UsesTheRightSender()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();
            eitherInletPipe.LeftInlet.ConnectTo(mockLeftPipe.Object.Outlets.Single());
            eitherInletPipe.RightInlet.ConnectTo(mockRightPipe.Object.Outlets.Single());

            Func<int> leftSender = () => 3;
            Func<int> rightSender = () => 4;
            mockLeftPipe.Setup(p => p.FindSender()).Returns(leftSender);
            mockRightPipe.Setup(p => p.FindSender()).Returns(rightSender);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Right);

            // Act
            var sender = eitherInletPipe.FindSender();

            // Assert
            sender.Should().Be(rightSender);
        }
    }
}
