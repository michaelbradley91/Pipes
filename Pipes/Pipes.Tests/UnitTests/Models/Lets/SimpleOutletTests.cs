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
    public class SimpleOutletTests
    {
        private IBasicPipe<int> pipe;
        private ISimpleInlet<int> inlet;
        private SimpleOutlet<int> simpleOutlet;
        
        [SetUp]
        public void SetUp()
        {
            pipe = PipeBuilder.New.BasicPipe<int>().Build();
            inlet = pipe.Inlet;
            simpleOutlet = (SimpleOutlet<int>) pipe.Outlet;
        }

        [Test]
        public void Outlet_BeginsDisconnectedFromAnyInlet()
        {
            // Assert
            simpleOutlet.ConnectedInlet.Should().BeNull();
        }

        [Test]
        public void Outlet_RemembersThePipeItIsConnectedTo()
        {
            // Assert
            simpleOutlet.Pipe.Should().Be(pipe);
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
                    simpleOutlet.Receive();
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
                receivedMessage = simpleOutlet.Receive();
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
                messageReceived = simpleOutlet.Receive();
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
                    simpleOutlet.Receive(TimeSpan.FromMilliseconds(50));
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
                receivedMessage = simpleOutlet.Receive(TimeSpan.FromMilliseconds(5000));
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
                messageReceived = simpleOutlet.Receive(TimeSpan.FromMilliseconds(50));
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
                    simpleOutlet.ReceiveImmediately();
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
                messageReceived = simpleOutlet.ReceiveImmediately();
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
            simpleOutlet.Receive(TimeSpan.FromMilliseconds(-1));
        }

        [Test]
        public void ConnectTo_GivenTheOutletIsNotConnected_ConnectsToTheGivenInlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            simpleOutlet.ConnectTo(pipe2.Inlet);

            // Assert
            simpleOutlet.ConnectedInlet.Should().Be(pipe2.Inlet);
            pipe2.Inlet.ConnectedOutlet.Should().Be(simpleOutlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_GivenTheOutletIsAlreadyConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(simpleOutlet);
            var pipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            simpleOutlet.ConnectTo(pipe3.Inlet);
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
                    simpleOutlet.Receive();
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
                simpleOutlet.ConnectTo(pipe2.Inlet);
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
            pipe2.Inlet.ConnectTo(simpleOutlet);

            // Act
            simpleOutlet.Receive();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveWithATimeout_GivenTheOutletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(simpleOutlet);

            // Act
            simpleOutlet.Receive(TimeSpan.FromMilliseconds(100));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ReceiveImmediately_GivenTheOutletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(simpleOutlet);

            // Act
            simpleOutlet.ReceiveImmediately();
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
                messageReceived = simpleOutlet.Receive();
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
                messageReceived = simpleOutlet.Receive(TimeSpan.FromMilliseconds(2000));
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
            var messageReceived = simpleOutlet.ReceiveImmediately();
            Thread.Sleep(500);

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingThePipeToItself_ThrowsAnInvalidOperationException()
        {
            // Act
            simpleOutlet.ConnectTo(inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingPipesToCreateACycle_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);

            // Act
            simpleOutlet.ConnectTo(pipe2.Inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Disconnect_GivenTheOutletIsNotConnected_ThrowsAnInvalidOperationException()
        {
            // Act
            simpleOutlet.Disconnect();
        }

        [Test]
        public void Disconnect_GivenTheOutletIsConnected_DisconnectsTheOutletFromItsInlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            simpleOutlet.ConnectTo(pipe2.Inlet);

            // Act
            simpleOutlet.Disconnect();

            // Assert
            simpleOutlet.ConnectedInlet.Should().BeNull();
            pipe2.Inlet.ConnectedOutlet.Should().BeNull();
        }
    }
}
