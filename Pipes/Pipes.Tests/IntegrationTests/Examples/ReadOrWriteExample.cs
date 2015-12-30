using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.Utilities;

namespace Pipes.Tests.IntegrationTests.Examples
{
    /// <summary>
    /// An example of a process that will read or write to a pipe - whichever it can do first.
    /// </summary>
    [TestFixture]
    public class ReadOrWriteExample
    {
        private ISimpleOutlet<int> externalReceiverOutlet;
        private ISimpleInlet<int> externalSenderInlet;

        private ISimpleInlet<int> prepareSendInlet;

        private ISimpleOutlet<Either<int, int>> resultOutlet;

        [SetUp]
        public void SetUp()
        {
            var splittingPipe = PipeBuilder.New.SplittingPipe<int>().Build();
            var eitherInletPipe = PipeBuilder.New.EitherInletPipe<Either<int, int>>().WithRandomisingTieBreaker().Build();
            var capacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(1).Build();
            var leftTransformPipe = PipeBuilder.New.TransformPipe<int, Either<int, int>>().WithMap(Either<int, int>.CreateLeft).Build();
            var rightTransformPipe = PipeBuilder.New.TransformPipe<int, Either<int, int>>().WithMap(Either<int, int>.CreateRight).Build();
            
            capacityPipe.Outlet.ConnectTo(splittingPipe.Inlet);
            splittingPipe.RightOutlet.ConnectTo(leftTransformPipe.Inlet);
            leftTransformPipe.Outlet.ConnectTo(eitherInletPipe.LeftInlet);
            rightTransformPipe.Outlet.ConnectTo(eitherInletPipe.RightInlet);

            externalReceiverOutlet = splittingPipe.LeftOutlet;
            externalSenderInlet = rightTransformPipe.Inlet;
            prepareSendInlet = capacityPipe.Inlet;
            resultOutlet = eitherInletPipe.Outlet;
        }

        [Test]
        public void WhenThereIsOnlyAnExternalSender_TheMessageCanBeReceived()
        {
            // Arrange
            const int message = 14;
            RunInThread(() => externalSenderInlet.Send(message));

            // Act
            prepareSendInlet.Send(1);
            var result = resultOutlet.Receive();

            // Assert
            result.IsRight.Should().BeTrue(); // I read the message
            result.GetRight().Should().Be(message);
        }

        [Test]
        public void WhenThereIsOnlyAnExternalReceiver_YourMessageCanBeReceived()
        {
            // Arrange
            var externallyReceivedMessage = 0;
            RunInThread(() => externallyReceivedMessage = externalReceiverOutlet.Receive());

            // Act
            const int message = 131;
            prepareSendInlet.Send(message);
            var result = resultOutlet.Receive();

            // Assert
            Thread.Sleep(500); // wait for variable assignment.
            result.IsLeft.Should().BeTrue();
            result.GetLeft().Should().Be(message); // I wrote the message
            externallyReceivedMessage.Should().Be(message);
        }

        private static void RunInThread(ThreadStart threadStart)
        {
            var thread = new Thread(threadStart);
            thread.Start();
        }
    }
}
