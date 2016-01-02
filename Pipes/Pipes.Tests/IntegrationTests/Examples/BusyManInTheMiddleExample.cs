using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Tests.Helpers;

namespace Pipes.Tests.IntegrationTests.Examples
{
    [TestFixture]
    public class BusyManInTheMiddleExample
    {
        [Test]
        public void Test()
        {
            const int numberOfMessages = 1000;

            var basicPipe = PipeBuilder.New.BasicPipe<IReadOnlyList<int>>().Build();
            var eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var valvedPipe = PipeBuilder.New.ValvedPipe<int, int>().WithRandomisingTieBreaker().Build();
            valvedPipe.Inlet.ConnectTo(eitherOutletPipe.RightOutlet);

            // Sender
            ThreadHelpers.RunInThread(() =>
            {
                for (var i = 0; i < numberOfMessages; i++)
                {
                    eitherOutletPipe.Inlet.Send(i);
                }
            });

            // Receiver
            ThreadHelpers.RunInThread(() =>
            {
                var receivedMessages = new List<int>();
                for (var i = 0; i < numberOfMessages; i++)
                {
                    var message = valvedPipe.Outlet.Receive();
                    receivedMessages.Add(message);
                }
                basicPipe.Inlet.Send(receivedMessages);
            });

            // Man in the middle
            ThreadHelpers.RunInThread(() =>
            {
                var messageQueue = new Queue<int>();
                var numberOfMessagesSent = 0;
                while (numberOfMessagesSent != numberOfMessages)
                {
                    if (messageQueue.Count == 0)
                    {
                        var message = eitherOutletPipe.LeftOutlet.Receive();
                        messageQueue.Enqueue(message);
                    }
                    else
                    {
                        var result = valvedPipe.Valve.ReceiveOrSend(messageQueue.Peek());
                        if (result.MessageReceived)
                        {
                            messageQueue.Enqueue(result.GetReceivedMessage());
                        }
                        else
                        {
                            numberOfMessagesSent++;
                            messageQueue.Dequeue();
                        }
                    }
                }
            });

            var allMessages = basicPipe.Outlet.Receive();

            allMessages.Should().HaveCount(numberOfMessages);
            for (var i = 0; i < numberOfMessages; i++)
            {
                allMessages[i].Should().Be(i);
            }
        }
        
    }
}
