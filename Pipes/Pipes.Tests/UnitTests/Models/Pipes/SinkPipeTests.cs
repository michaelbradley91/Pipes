using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class SinkPipeTests
    {
        private ISinkPipe<int> sinkPipe;

        [SetUp]
        public void SetUp()
        {
            sinkPipe = PipeBuilder.New.SinkPipe<int>().Build();
        }

        [Test]
        public void SinkPipe_HasOneInlet()
        {
            // Assert
            sinkPipe.Inlet.Should().NotBeNull();
            sinkPipe.AllInlets.Count.Should().Be(1);
            sinkPipe.AllInlets.Single().Should().Be(sinkPipe.Inlet);
        }

        [Test]
        public void SinkPipe_HasZeroOutlets()
        {
            // Assert
            sinkPipe.AllOutlets.Should().BeEmpty();
        }

        [Test]
        public void FindReceiver_AlwaysReturnsAReceiver()
        {
            // Act
            var receiver = sinkPipe.FindReceiver(sinkPipe.Inlet);

            // Assert
            receiver.Should().NotBeNull();
        }
        
        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindSender_AsThereAreNoOutletsOnThePipe_ThrowsAnException()
        {
            // Arrange
            var outlet = new Mock<IOutlet<int>>().Object;

            // Act
            sinkPipe.FindSender(outlet);
        }
    }
}
