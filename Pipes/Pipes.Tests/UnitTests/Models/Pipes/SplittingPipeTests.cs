using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class SplittingPipeTests
    {
        private ISplittingPipe<int> splittingPipe;
        
        [SetUp]
        public void SetUp()
        {
            splittingPipe = PipeBuilder.New.SplittingPipe<int>().Build();
        }

        [Test]
        public void SplittingPipe_HasOneInlet()
        {
            // Assert
            splittingPipe.Inlet.Should().NotBeNull();
            splittingPipe.AllInlets.Count.Should().Be(1);
            splittingPipe.AllInlets.Single().Should().Be(splittingPipe.Inlet);
        }

        [Test]
        public void SplittingPipe_HasTwoOutlets()
        {
            // Assert
            splittingPipe.LeftOutlet.Should().NotBeNull();
            splittingPipe.RightOutlet.Should().NotBeNull();
            splittingPipe.AllOutlets.Count.Should().Be(2);
            splittingPipe.AllOutlets.Should().BeEquivalentTo(splittingPipe.LeftOutlet, splittingPipe.RightOutlet);
        }

        [Test]
        public void FindReceiver_GivenThereIsNoReceiver_ReturnsNull()
        {
            // Act
            var receiver = splittingPipe.FindReceiver(splittingPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnTheLeftOutletButNotTheRightOutlet_ReturnsNull()
        {
            // Arrange
            var thread = new Thread(() =>
            {
                splittingPipe.LeftOutlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = splittingPipe.FindReceiver(splittingPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
            thread.Abort();
        }

        [Test]
        public void FindReceiver_GivenThereIsAReceiverOnTheRightOutletButNotTheLeftOutlet_ReturnsNull()
        {
            // Arrange
            var thread = new Thread(() =>
            {
                splittingPipe.RightOutlet.Receive();
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var receiver = splittingPipe.FindReceiver(splittingPipe.Inlet);

            // Assert
            receiver.Should().BeNull();
            thread.Abort();
        }

        [Test]
        public void FindReceiver_GivenThereAreReceiversOnBothOutlets_ReturnsAReceiverToPassTheMessageToBoth()
        {
            // Arrange
            const int message = 3;

            var leftReceivedMessage = default(int);
            var leftReceiver = new Thread(() =>
            {
                leftReceivedMessage = splittingPipe.LeftOutlet.Receive();
            });
            leftReceiver.Start();
            Thread.Sleep(500);

            var rightReceivedMessage = default(int);
            var rightReceiver = new Thread(() =>
            {
                rightReceivedMessage = splittingPipe.RightOutlet.Receive();
            });
            rightReceiver.Start();
            Thread.Sleep(500);

            // Act
            var receiver = splittingPipe.FindReceiver(splittingPipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();

            // Act
            receiver(message);
            Thread.Sleep(500);

            // Assert
            leftReceivedMessage.Should().Be(message);
            rightReceivedMessage.Should().Be(message);
        }

        [Test]
        public void FindSender_GivenThereIsNoSender_ReturnsNull()
        {
            // Act
            var sender = splittingPipe.FindSender(splittingPipe.LeftOutlet);

            // Assert
            sender.Should().BeNull();
        }

        [Test]
        public void FindSender_GivenThereIsASenderButNoSecondReceiver_ReturnsNull()
        {
            // Arrange
            const int message = 3;
            var thread = new Thread(() =>
            {
                splittingPipe.Inlet.Send(message);
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            var sender = splittingPipe.FindSender(splittingPipe.LeftOutlet);

            // Assert
            sender.Should().BeNull();
            thread.Abort();
        }

        [Test]
        public void FindSender_GivenThereIsASenderButNoSecondReceiverOnTheOppositeOutlet_ReturnsNull()
        {
            // Arrange
            const int message = 3;
            var senderThread = new Thread(() =>
            {
                splittingPipe.Inlet.Send(message);
            });
            senderThread.Start();
            Thread.Sleep(500);

            var leftReceiver = new Thread(() =>
            {
                splittingPipe.LeftOutlet.Receive();
            });
            leftReceiver.Start();
            Thread.Sleep(500);

            // Act
            var sender = splittingPipe.FindSender(splittingPipe.LeftOutlet);

            // Assert
            sender.Should().BeNull();
            senderThread.Abort();
            leftReceiver.Abort();
        }

        [Test]
        public void FindSender_GivenThereIsASenderAndAReceiverOnTheOppositeOutlet_ReturnsThatSender()
        {
            // Arrange
            const int message = 3;
            var senderThread = new Thread(() =>
            {
                splittingPipe.Inlet.Send(message);
            });
            senderThread.Start();
            Thread.Sleep(500);

            var leftReceivedMessage = default(int);
            var leftReceiver = new Thread(() =>
            {
                leftReceivedMessage = splittingPipe.LeftOutlet.Receive();
            });
            leftReceiver.Start();
            Thread.Sleep(500);

            // Act
            var sender = splittingPipe.FindSender(splittingPipe.RightOutlet);

            // Assert
            sender.Should().NotBeNull();

            // Act
            var rightReceivedMessage = sender();
            Thread.Sleep(500);

            // Assert
            rightReceivedMessage.Should().Be(message);
            leftReceivedMessage.Should().Be(message);
        }
    }
}
