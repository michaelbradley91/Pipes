using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Lets
{
    [TestFixture]
    public class OutletTests
    {
        private IBasicPipe<int> pipe;
        private Inlet<int> inlet;
        private Outlet<int> outlet;
        
        [SetUp]
        public void SetUp()
        {
            pipe = PipeBuilder.New.BasicPipe<int>().Build();
            inlet = pipe.Inlet;
            outlet = pipe.Outlet;
        }

        [Test]
        public void Outlet_BeginsDisconnectedFromAnyInlet()
        {
            // Assert
            outlet.ConnectedInlet.Should().BeNull();
        }

        [Test]
        public void Outlet_RemembersThePipeItIsConnectedTo()
        {
            // Assert
            outlet.Pipe.Should().Be(pipe);
        }

        [Test]
        public void ReceiveWithoutATimeout_GivenThereIsNoSender_WillKeepWaitingUntilTheThreadIsInterrupted()
        {
            // Arrange
            var receivedSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    outlet.Receive();
                    receivedSuccessfully = true;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
            });
            
            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();
            caughtException.Should().BeNull();

            // Act
            thread.Interrupt();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<ThreadInterruptedException>();
        }

        [Test]
        public void ReceiveWithoutATimeout_GivenThereIsEventuallyASender_WillReceiveSuccessfully()
        {
            // Arrange
            const int sentMessage = 3;
            var receivedSuccessfully = false;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = outlet.Receive();
                receivedSuccessfully = true;
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();

            // Act
            inlet.Send(sentMessage);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(sentMessage);
            receivedSuccessfully.Should().BeTrue();
        }

        [Test]
        public void ReceiveWithoutATimeout_GivenThereIsASenderAlready_WillReceiveSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var receivedSuccessfully = false;
            var messageReceived = default(int);
            var sendingThread = new Thread(() =>
            {
                inlet.Send(sentMessage);
            });
            sendingThread.Start();
            Thread.Sleep(500);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.Receive();
                receivedSuccessfully = true;
            });

            // Act
            receivingThread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [Test]
        public void ReceiveWithATimeout_GivenThereIsNoSender_WillThrowATimeoutExceptionOnceTheTimeoutExpires()
        {
            // Arrange
            var receivedSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    outlet.Receive(TimeSpan.FromMilliseconds(50));
                    receivedSuccessfully = true;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<TimeoutException>();
        }

        [Test]
        public void ReceiveWithATimeout_GivenThereIsASenderWithinTheTimeout_WillReceiveSuccessfully()
        {
            // Arrange
            const int sentMessage = 3;
            var receivedSuccessfully = false;
            var receivedMessage = default(int);
            var thread = new Thread(() =>
            {
                receivedMessage = outlet.Receive(TimeSpan.FromMilliseconds(5000));
                receivedSuccessfully = true;
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();

            // Act
            inlet.Send(sentMessage);
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(sentMessage);
            receivedSuccessfully.Should().BeTrue();
        }

        [Test]
        public void ReceiveWithATimeout_GivenThereIsASenderAlready_WillReceiveSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var receivedSuccessfully = false;
            var messageReceived = default(int);
            var sendingThread = new Thread(() =>
            {
                inlet.Send(sentMessage);
            });
            sendingThread.Start();
            Thread.Sleep(500);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.Receive(TimeSpan.FromMilliseconds(50));
                receivedSuccessfully = true;
            });

            // Act
            receivingThread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [Test]
        public void ReceiveImmediately_GivenThereIsNoSender_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var receivedSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    outlet.ReceiveImmediately();
                    receivedSuccessfully = true;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<InvalidOperationException>();
        }

        [Test]
        public void ReceiveImmediately_GivenThereIsAlreadyASender_ReceivesSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var receivedSuccessfully = false;
            var messageReceived = default(int);
            var sendingThread = new Thread(() =>
            {
                inlet.Send(sentMessage);
            });
            sendingThread.Start();
            Thread.Sleep(500);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.ReceiveImmediately();
                receivedSuccessfully = true;
            });
            // Act
            receivingThread.Start();
            Thread.Sleep(500);

            // Assert
            receivedSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void ReceiveWithTimeout_GivenANegativeTimeSpan_ThrowsAnArgumentOutOfRangeException()
        {
            // Act
            outlet.Receive(TimeSpan.FromMilliseconds(-1));
        }

        [Test]
        public void ConnectTo_GivenTheOutletIsNotConnected_ConnectsToTheGivenInlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            outlet.ConnectTo(pipe2.Inlet);

            // Assert
            outlet.ConnectedInlet.Should().Be(pipe2.Inlet);
            pipe2.Inlet.ConnectedOutlet.Should().Be(outlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_GivenTheOutletIsAlreadyConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(outlet);
            var pipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            outlet.ConnectTo(pipe3.Inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_GivenTheOutletHasASender_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var thread = new Thread(() =>
            {
                try
                {
                    outlet.Receive();
                }
                catch
                {
                    // ignored
                }
            });
            thread.Start();
            Thread.Sleep(500);

            // Act
            try
            {
                outlet.ConnectTo(pipe2.Inlet);
            }
            catch
            {
                thread.Interrupt();
                throw;
            }
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveWithoutATimeout_GivenTheOutletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(outlet);

            // Act
            outlet.Receive();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveWithATimeout_GivenTheOutletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(outlet);

            // Act
            outlet.Receive(TimeSpan.FromMilliseconds(100));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveImmediately_GivenTheOutletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(outlet);

            // Act
            outlet.ReceiveImmediately();
        }

        [Test]
        public void ReceiveWithoutATimeout_GivenTheInletIsConnected_WillReceiveFromTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            inlet.ConnectTo(pipe2.Outlet);

            const int messageSent = 4;
            var messageReceived = default(int);
            new Thread(() =>
            {
                messageReceived = outlet.Receive();
            }).Start();

            // Act
            pipe2.Inlet.Send(messageSent);
            Thread.Sleep(500);

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [Test]
        public void ReceiveWithATimeout_GivenTheInletIsConnected_WillReceiveFromTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            inlet.ConnectTo(pipe2.Outlet);

            const int messageSent = 4;
            var messageReceived = default(int);
            new Thread(() =>
            {
                messageReceived = outlet.Receive(TimeSpan.FromMilliseconds(2000));
            }).Start();

            // Act
            pipe2.Inlet.Send(messageSent);
            Thread.Sleep(500);

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [Test]
        public void ReceiveImmediately_GivenTheInletIsConnected_WillReceiveFromTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            inlet.ConnectTo(pipe2.Outlet);

            const int messageSent = 4;
            new Thread(() =>
            {
                pipe2.Inlet.Send(messageSent);
            }).Start();
            Thread.Sleep(500);

            // Act
            var messageReceived = outlet.ReceiveImmediately();
            Thread.Sleep(500);

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingThePipeToItself_ThrowsAnInvalidOperationException()
        {
            // Act
            outlet.ConnectTo(inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingPipesToCreateACycle_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);

            // Act
            outlet.ConnectTo(pipe2.Inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Disconnect_GivenTheOutletIsNotConnected_ThrowsAnInvalidOperationException()
        {
            // Act
            outlet.Disconnect();
        }

        [Test]
        public void Disconnect_GivenTheOutletIsConnected_DisconnectsTheOutletFromItsInlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            outlet.ConnectTo(pipe2.Inlet);

            // Act
            outlet.Disconnect();

            // Assert
            outlet.ConnectedInlet.Should().BeNull();
            pipe2.Inlet.ConnectedOutlet.Should().BeNull();
        }
    }
}
