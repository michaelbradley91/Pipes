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
    public class EitherOutletPipeTests
    {
        private Mock<ITieBreaker> tieBreaker;
        private IEitherOutletPipe<ITieBreaker, int> eitherOutletPipe;
        
        [SetUp]
        public void SetUp()
        {
            tieBreaker = new Mock<ITieBreaker>();
            eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().WithTieBreaker(tieBreaker.Object).Build();
        }

        [Test]
        public void EitherOutletPipe_HasOneInlet()
        {
            // Assert
            eitherOutletPipe.Inlet.Should().NotBeNull();
            eitherOutletPipe.Inlets.Count.Should().Be(1);
            eitherOutletPipe.Inlets.Single().Should().Be(eitherOutletPipe.Inlet);
        }

        [Test]
        public void EitherOutletPipe_HasTwoOutlets()
        {
            // Assert
            eitherOutletPipe.LeftOutlet.Should().NotBeNull();
            eitherOutletPipe.RightOutlet.Should().NotBeNull();
            eitherOutletPipe.Outlets.Count.Should().Be(2);
            eitherOutletPipe.Outlets.Should().BeEquivalentTo(eitherOutletPipe.LeftOutlet, eitherOutletPipe.RightOutlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = eitherOutletPipe.FindReceiver();

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
            var receiver = eitherOutletPipe.FindReceiver();

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
            var receiver = eitherOutletPipe.FindReceiver();

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
            var sender = eitherOutletPipe.FindSender();

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
            var sender = eitherOutletPipe.FindSender();

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
            eitherOutletPipe.LeftOutlet.ConnectTo(mockPipe.Object.Inlets.Single());

            // Act
            eitherOutletPipe.FindReceiver();

            // Assert
            mockPipe.Verify(p => p.FindReceiver());
        }

        [Test]
        public void FindReceiver_GivenThereIsAPipeConnectedToItsRightOutlet_AsksThatPipeForAReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            eitherOutletPipe.RightOutlet.ConnectTo(mockPipe.Object.Inlets.Single());

            // Act
            eitherOutletPipe.FindReceiver();

            // Assert
            mockPipe.Verify(p => p.FindReceiver());
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            eitherOutletPipe.Inlet.ConnectTo(mockPipe.Object.Outlets.Single());

            // Act
            eitherOutletPipe.FindSender();

            // Assert
            mockPipe.Verify(p => p.FindSender());
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnBothOutletsAndTheTieBreakerResolvesToLeft_UsesTheLeftReceiver()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();
            eitherOutletPipe.LeftOutlet.ConnectTo(mockLeftPipe.Object.Inlets.Single());
            eitherOutletPipe.RightOutlet.ConnectTo(mockRightPipe.Object.Inlets.Single());

            Action<int> leftReceiver = _ => { };
            Action<int> rightReceiver = _ => { };
            mockLeftPipe.Setup(p => p.FindReceiver()).Returns(leftReceiver);
            mockRightPipe.Setup(p => p.FindReceiver()).Returns(rightReceiver);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Left);

            // Act
            var sender = eitherOutletPipe.FindReceiver();

            // Assert
            sender.Should().Be(leftReceiver);
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnBothOutletsAndTheTieBreakerResolvesToRight_UsesTheRightReceiver()
        {
            // Arrange
            var mockLeftPipe = PipeHelpers.CreateMockPipe<int>();
            var mockRightPipe = PipeHelpers.CreateMockPipe<int>();
            eitherOutletPipe.LeftOutlet.ConnectTo(mockLeftPipe.Object.Inlets.Single());
            eitherOutletPipe.RightOutlet.ConnectTo(mockRightPipe.Object.Inlets.Single());

            Action<int> leftReceiver = _ => { };
            Action<int> rightReceiver = _ => { };
            mockLeftPipe.Setup(p => p.FindReceiver()).Returns(leftReceiver);
            mockRightPipe.Setup(p => p.FindReceiver()).Returns(rightReceiver);

            tieBreaker.Setup(t => t.ResolveTie()).Returns(TieResult.Right);

            // Act
            var sender = eitherOutletPipe.FindReceiver();

            // Assert
            sender.Should().Be(rightReceiver);
        }
    }
}
