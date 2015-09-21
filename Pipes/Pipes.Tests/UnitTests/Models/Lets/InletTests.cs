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
    public class InletTests
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
        public void Inlet_BeginsDisconnectedFromAnyOutlet()
        {
            // Assert
            inlet.ConnectedOutlet.Should().BeNull();
        }

        [Test]
        public void Inlet_RemembersThePipeItIsConnectedTo()
        {
            // Assert
            inlet.Pipe.Should().Be(pipe);
        }

        [Test]
        public void SendWithoutATimeout_GivenThereIsNoReceiver_WillKeepWaitingUntilTheThreadIsInterrupted()
        {
            // Arrange
            var sentSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    inlet.Send(3);
                    sentSuccessfully = true;
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
            sentSuccessfully.Should().BeFalse();
            caughtException.Should().BeNull();

            // Act
            thread.Interrupt();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<ThreadInterruptedException>();
        }

        [Test]
        public void SendWithoutATimeout_GivenThereIsEventuallyAReceiver_WillSendSuccessfully()
        {
            // Arrange
            const int sentMessage = 3;
            var sentSuccessfully = false;
            var thread = new Thread(() =>
            {
                inlet.Send(sentMessage);
                sentSuccessfully = true;
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeFalse();

            // Act
            var receivedMessage = outlet.Receive();
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(sentMessage);
            sentSuccessfully.Should().BeTrue();
        }

        [Test]
        public void SendWithoutATimeout_GivenThereIsAReceiverAlready_WillSendSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var sentSuccessfully = false;
            var messageReceived = default(int);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.Receive();
            });
            receivingThread.Start();
            Thread.Sleep(500);
            var sendingThread = new Thread(() =>
            {
                inlet.Send(sentMessage);
                sentSuccessfully = true;
            });

            // Act
            sendingThread.Start();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [Test]
        public void SendWithATimeout_GivenThereIsNoReceiver_WillThrowATimeoutExceptionOnceTheTimeoutExpires()
        {
            // Arrange
            var sentSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    inlet.Send(3, TimeSpan.FromMilliseconds(50));
                    sentSuccessfully = true;
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
            sentSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<TimeoutException>();
        }

        [Test]
        public void SendWithATimeout_GivenThereIsAReceiveWithinTheTimeout_WillSendSuccessfully()
        {
            // Arrange
            const int sentMessage = 3;
            var sentSuccessfully = false;
            var thread = new Thread(() =>
            {
                inlet.Send(sentMessage, TimeSpan.FromMilliseconds(5000));
                sentSuccessfully = true;
            });

            // Act
            thread.Start();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeFalse();

            // Act
            var receivedMessage = outlet.Receive();
            Thread.Sleep(500);

            // Assert
            receivedMessage.Should().Be(sentMessage);
            sentSuccessfully.Should().BeTrue();
        }

        [Test]
        public void SendWithATimeout_GivenThereIsAReceiverAlready_WillSendSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var sentSuccessfully = false;
            var messageReceived = default(int);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.Receive();
            });
            receivingThread.Start();
            Thread.Sleep(500);
            var sendingThread = new Thread(() =>
            {
                inlet.Send(sentMessage, TimeSpan.FromMilliseconds(50));
                sentSuccessfully = true;
            });

            // Act
            sendingThread.Start();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [Test]
        public void SendImmediately_GivenThereIsNoReceiver_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var sentSuccessfully = false;
            var caughtException = default(Exception);
            var thread = new Thread(() =>
            {
                try
                {
                    inlet.SendImmediately(3);
                    sentSuccessfully = true;
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
            sentSuccessfully.Should().BeFalse();
            caughtException.Should().NotBeNull().And.BeAssignableTo<InvalidOperationException>();
        }

        [Test]
        public void SendImmediately_GivenThereIsAlreadyAReceiver_SendsSuccessfully()
        {
            // Arrange
            const int sentMessage = 4;
            var sentSuccessfully = false;
            var messageReceived = default(int);
            var receivingThread = new Thread(() =>
            {
                messageReceived = outlet.Receive();
            });
            receivingThread.Start();
            Thread.Sleep(500);
            var sendingThread = new Thread(() =>
            {
                inlet.SendImmediately(sentMessage);
                sentSuccessfully = true;
            });

            // Act
            sendingThread.Start();
            Thread.Sleep(500);

            // Assert
            sentSuccessfully.Should().BeTrue();
            messageReceived.Should().Be(sentMessage);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void SendWithTimeout_GivenANegativeTimeSpan_ThrowsAnArgumentOutOfRangeException()
        {
            // Act
            inlet.Send(3, TimeSpan.FromMilliseconds(-1));
        }

        [Test]
        public void ConnectTo_GivenTheInletIsNotConnected_ConnectsToTheGivenOutlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            inlet.ConnectTo(pipe2.Outlet);

            // Assert
            inlet.ConnectedOutlet.Should().Be(pipe2.Outlet);
            pipe2.Outlet.ConnectedInlet.Should().Be(inlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_GivenTheInletIsAlreadyConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);
            var pipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            inlet.ConnectTo(pipe3.Outlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_GivenTheInletHasASender_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var thread = new Thread(() =>
            {
                try
                {
                    inlet.Send(3);
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
                inlet.ConnectTo(pipe2.Outlet);
            }
            catch
            {
                thread.Interrupt();
                throw;
            }
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void SendWithoutATimeout_GivenTheInletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);

            // Act
            inlet.Send(3);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void SendWithATimeout_GivenTheInletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);

            // Act
            inlet.Send(3, TimeSpan.FromMilliseconds(100));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void SendImmediately_GivenTheInletIsConnected_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Outlet.ConnectTo(inlet);

            // Act
            inlet.SendImmediately(4);
        }

        [Test]
        public void SendWithoutATimeout_GivenTheOutletIsConnected_WillSendToTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            outlet.ConnectTo(pipe2.Inlet);

            const int messageSent = 4;
            new Thread(() =>
            {
                inlet.Send(messageSent);
            }).Start();

            // Act
            var messageReceived = pipe2.Outlet.Receive();

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [Test]
        public void SendWithATimeout_GivenTheOutletIsConnected_WillSendToTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            outlet.ConnectTo(pipe2.Inlet);

            const int messageSent = 4;
            new Thread(() =>
            {
                inlet.Send(messageSent, TimeSpan.FromMilliseconds(1000));
            }).Start();

            // Act
            var messageReceived = pipe2.Outlet.Receive();

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [Test]
        public void SendImmediately_GivenTheOutletIsConnected_WillSendToTheConnectedPipe()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            outlet.ConnectTo(pipe2.Inlet);

            const int messageSent = 4;
            var messageReceived = default(int);
            new Thread(() =>
            {
                messageReceived = pipe2.Outlet.Receive();
            }).Start();
            Thread.Sleep(500);

            // Act
            inlet.SendImmediately(messageSent);
            Thread.Sleep(500);

            // Assert
            messageReceived.Should().Be(messageSent);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingThePipeToItself_ThrowsAnInvalidOperationException()
        {
            // Act
            inlet.ConnectTo(outlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void ConnectTo_WhenConnectingPipesToCreateACycle_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            pipe2.Inlet.ConnectTo(outlet);

            // Act
            inlet.ConnectTo(pipe2.Outlet);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void Disconnect_GivenTheInletIsNotConnected_ThrowsAnInvalidOperationException()
        {
            // Act
            inlet.Disconnect();
        }

        [Test]
        public void Disconnect_GivenTheInletIsConnected_DisconnectsTheInletFromItsOutlet()
        {
            // Arrange
            var pipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            inlet.ConnectTo(pipe2.Outlet);

            // Act
            inlet.Disconnect();

            // Assert
            inlet.ConnectedOutlet.Should().BeNull();
            pipe2.Outlet.ConnectedInlet.Should().BeNull();
        }
    }
}
