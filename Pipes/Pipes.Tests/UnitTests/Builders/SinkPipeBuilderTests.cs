using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SinkPipeBuilderTests
    {
        private ISinkPipeBuilder<int> sinkPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            sinkPipeBuilder = new SinkPipeBuilder<int>();
        }

        [Test]
        public void Build_ReturnsASinkPipe()
        {
            // Act
            var pipe = sinkPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
        }
    }
}