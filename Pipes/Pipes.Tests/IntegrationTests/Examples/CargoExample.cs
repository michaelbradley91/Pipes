using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.IntegrationTests.Examples
{
    [TestFixture]
    public class CargoExample
    {
        private ICapacityPipe<int> cargoShip;
        private ISourcePipe<int> producer;
        private ISinkPipe<int> consumer;

        [SetUp]
        public void SetUp()
        {
            cargoShip = PipeBuilder.New.CapacityPipe<int>().WithCapacity(1000).Build();
            producer = PipeBuilder.New.SourcePipe<int>().WithMessageProducer(new Producer().Produce).Build();
            consumer = PipeBuilder.New.SinkPipe<int>().Build();
        }

        [Test]
        public void Test()
        {
            cargoShip.Inlet.ConnectTo(producer.Outlet);
            cargoShip.Inlet.Disconnect();

            cargoShip.StoredMessages.Count.Should().Be(cargoShip.Capacity);
            cargoShip.StoredMessages[4].Should().Be(4);

            cargoShip.Outlet.ConnectTo(consumer.Inlet);
            cargoShip.Outlet.Disconnect();

            cargoShip.Inlet.ConnectTo(producer.Outlet);
            cargoShip.Inlet.Disconnect();

            cargoShip.StoredMessages.Count.Should().Be(cargoShip.Capacity);
            cargoShip.StoredMessages[4].Should().Be(1004);

            cargoShip.Outlet.ConnectTo(consumer.Inlet);
            cargoShip.Outlet.Disconnect();

            cargoShip.StoredMessages.Count.Should().Be(0);
        }

        private class Producer
        {
            private int count;
            public int Produce()
            {
                return count++;
            }
        }
    }
}
