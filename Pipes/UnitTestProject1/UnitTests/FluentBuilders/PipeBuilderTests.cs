using FluentAssertions;
using NUnit.Framework;
using Pipes.FluentBuilders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class PipeBuilderTests
    {
        private IPipeBuilder pipeBuilder;

        [SetUp]
        public void SetUp()
        {
            pipeBuilder = new PipeBuilder();
        }

        [Test]
        public void Create_ReturnsAPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.Create;

            // Assert
            builder.Should().BeAssignableTo<PipeBuilder>();
        }

        [Test]
        public void NewPipe_ReturnsABasicPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.NewPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<BasicPipeBuilder<int>>();
        }

        [Test]
        public void NewTwoInletPipe_ReturnsATwoInletPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.NewTwoInletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<TwoInletPipeBuilder<int>>();
        }

        [Test]
        public void NewTwoOutletPipe_ReturnsATwoOutletPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.NewTwoOutletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<TwoOutletPipeBuilder<int>>();
        }
    }
}