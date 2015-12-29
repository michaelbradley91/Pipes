using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.IntegrationTests.Examples
{
    [TestFixture]
    public class DrainCapacityPipeExample
    {
        private ICapacityPipe<int> capacityPipe;
        private ISplittingPipe<int> splittingPipe;
        private ISinkPipe<int> sinkPipe1;
        private ISinkPipe<int> sinkPipe2;
        private ISourcePipe<int> sourcePipe;

        [SetUp]
        public void SetUp()
        {
            capacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(100).Build();
            splittingPipe = PipeBuilder.New.SplittingPipe<int>().Build();
            sinkPipe1 = PipeBuilder.New.SinkPipe<int>().Build();
            sinkPipe2 = PipeBuilder.New.SinkPipe<int>().Build();
            sourcePipe = PipeBuilder.New.SourcePipe<int>().WithMessageProducer(() => 1).Build();

            capacityPipe.Outlet.ConnectTo(splittingPipe.Inlet);
            splittingPipe.RightOutlet.ConnectTo(sinkPipe1.Inlet);
        }

        [Test]
        public void Test()
        {
            // Fill the capacity pipe
            sourcePipe.Outlet.ConnectTo(capacityPipe.Inlet);
            sourcePipe.Outlet.Disconnect();

            capacityPipe.StoredMessages.Count.Should().Be(capacityPipe.Capacity);

            // Enable messages to flow through the splitting pipe
            splittingPipe.LeftOutlet.ConnectTo(sinkPipe2.Inlet);

            // Check the capacity pipe was emptied
            capacityPipe.StoredMessages.Should().BeEmpty();
        }
    }
}
