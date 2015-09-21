using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class CapacityPipeTests
    {
        private ICapacityPipe<int> capacityZeroPipe;
        private ICapacityPipe<int> capacityOnePipe;
        private ICapacityPipe<int> capacityTwoPipe;
        private ICapacityPipe<int> capacityThreePipe;
        
        [SetUp]
        public void SetUp()
        {
            capacityZeroPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(0).Build();
            capacityOnePipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(1).Build();
            capacityTwoPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(2).Build();
            capacityThreePipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(3).Build();
        }

        [Test]
        public void CapacityPipe_HasOneInlet()
        {
            // Assert
            capacityZeroPipe.Inlet.Should().NotBeNull();
            capacityZeroPipe.Inlets.Count.Should().Be(1);
            capacityZeroPipe.Inlets.Single().Should().Be(capacityZeroPipe.Inlet);
        }

        [Test]
        public void CapacityPipe_HasOneOutlet()
        {
            // Assert
            capacityZeroPipe.Outlet.Should().NotBeNull();
            capacityZeroPipe.Outlets.Count.Should().Be(1);
            capacityZeroPipe.Outlets.Single().Should().Be(capacityZeroPipe.Outlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = capacityZeroPipe.FindReceiver();

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
                receivedMessage = capacityZeroPipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = capacityZeroPipe.FindReceiver();

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
            var sender = capacityZeroPipe.FindSender();

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
                capacityZeroPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = capacityZeroPipe.FindSender();

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
            capacityZeroPipe.Outlet.ConnectTo(mockPipe.Object.Inlets.Single());

            // Act
            capacityZeroPipe.FindReceiver();

            // Assert
            mockPipe.Verify(p => p.FindReceiver());
        }

        [Test]
        public void FindSender_GivenThereIsAPipeConnectedToItsInlet_AsksThatPipeForASender()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            capacityZeroPipe.Inlet.ConnectTo(mockPipe.Object.Outlets.Single());

            // Act
            capacityZeroPipe.FindSender();

            // Assert
            mockPipe.Verify(p => p.FindSender());
        }

        [Test]
        public void FindReceiver_GivenThePipeHasSpareCapacity_ReturnsAReceiver()
        {
            // Act
            var receiver = capacityThreePipe.FindReceiver();

            // Assert
            receiver.Should().NotBeNull();
        }

        [Test]
        public void FindReceiver_GivenThePipeHasAReceiverOnItsOutlet_PrefersThatReceiver()
        {
            // Arrange
            const int message = 3;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = capacityThreePipe.Outlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = capacityThreePipe.FindReceiver();

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(message);
        }

        [Test]
        public void FindReceiver_GivenThePipeHasAReceiverFromAConnectedPipe_PrefersThatReceiver()
        {
            // Arrange
            var mockPipe = PipeHelpers.CreateMockPipe<int>();
            capacityThreePipe.Outlet.ConnectTo(mockPipe.Object.Inlets.Single());
            var expectedReceiver = new Action<int>(m => { });
            mockPipe.Setup(p => p.FindReceiver()).Returns(expectedReceiver);

            // Act
            var actualReceiver = capacityThreePipe.FindReceiver();

            // Assert
            actualReceiver.Should().Be(expectedReceiver);
        }

        [Test]
        public void FindSender_GivenThePipeIsHoldingAMessage_ReturnsThatMessage()
        {
            // Arrange
            const int message = 14;
            capacityThreePipe.Inlet.Send(message);

            // Act
            var sender = capacityThreePipe.FindSender();

            // Assert
            sender.Should().NotBeNull();
            sender().Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThePipeIsHoldingMessages_ReturnsMessagesInTheFirstInFirstOutOrder()
        {
            // Arrange
            const int expectedFirstMessage = 1;
            const int expectedSecondMessage = 2;
            const int expectedThirdMessage = 3;
            capacityThreePipe.FindReceiver()(expectedFirstMessage);
            capacityThreePipe.FindReceiver()(expectedSecondMessage);
            capacityThreePipe.FindReceiver()(expectedThirdMessage);

            // Act
            var actualFirstMessage = capacityThreePipe.FindSender()();
            var actualSecondMessage = capacityThreePipe.FindSender()();
            var actualThirdMessage = capacityThreePipe.FindSender()();

            // Assert
            actualFirstMessage.Should().Be(expectedFirstMessage);
            actualSecondMessage.Should().Be(expectedSecondMessage);
            actualThirdMessage.Should().Be(expectedThirdMessage);
        }

        [Test]
        public void FindReceiver_GivenThePipeIsFull_ReturnsNull()
        {
            // Arrange
            capacityThreePipe.FindReceiver()(1);
            capacityThreePipe.FindReceiver()(2);
            capacityThreePipe.FindReceiver()(3);

            // Act
            var receiver = capacityThreePipe.FindReceiver();

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindSender_AppliedToConnectedCapacityPipes_ReturnsTheMessagesFromAllConnectedPipesInTheRightOrder()
        {
            // Arrange
            capacityThreePipe.Outlet.ConnectTo(capacityTwoPipe.Inlet);

            const int expectedFirstMessage = 1;
            const int expectedSecondMessage = 2;
            const int expectedThirdMessage = 3;
            const int expectedFourthMessage = 4;
            const int expectedFifthMessage = 5;
            const int expectedSixthMessage = 6;

            capacityThreePipe.FindReceiver()(expectedFirstMessage);
            capacityThreePipe.FindReceiver()(expectedSecondMessage);
            capacityThreePipe.FindReceiver()(expectedThirdMessage);
            capacityThreePipe.FindReceiver()(expectedFourthMessage);
            capacityThreePipe.FindReceiver()(expectedFifthMessage);

            var thread = new Thread(() => capacityThreePipe.Inlet.Send(expectedSixthMessage));
            thread.Start();
            Thread.Sleep(500);

            // Act
            var actualFirstMessage = capacityTwoPipe.FindSender()();
            var actualSecondMessage = capacityTwoPipe.FindSender()();
            var actualThirdMessage = capacityTwoPipe.FindSender()();
            var actualFourthMessage = capacityTwoPipe.FindSender()();
            var actualFifthMessage = capacityTwoPipe.FindSender()();
            var actualSixthMessage = capacityTwoPipe.FindSender()();

            // Assert
            actualFirstMessage.Should().Be(expectedFirstMessage);
            actualSecondMessage.Should().Be(expectedSecondMessage);
            actualThirdMessage.Should().Be(expectedThirdMessage);
            actualFourthMessage.Should().Be(expectedFourthMessage);
            actualFifthMessage.Should().Be(expectedFifthMessage);
            actualSixthMessage.Should().Be(expectedSixthMessage);
        }

    }
}
