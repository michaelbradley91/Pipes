using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

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
            var builder = PipeBuilder.New;

            // Assert
            builder.Should().BeAssignableTo<PipeBuilder>();
        }

        [Test]
        public void BasicPipe_ReturnsABasicPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.BasicPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IBasicPipeBuilder<int>>();
        }

        [Test]
        public void CapacityPipe_ReturnsACapacityPipePipeBuilder()
        {
            // Act
            var builder = pipeBuilder.CapacityPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<ICapacityPipeBuilder<int>>();
        }

        [Test]
        public void EitherInletPipe_ReturnsAnEitherInletPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.EitherInletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IEitherInletPipeBuilder<int>>();
        }

        [Test]
        public void EitherOutletPipe_ReturnsAnEitherOutletPipeBuilder()
        {
            // Act
            var builder = pipeBuilder.EitherOutletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IEitherOutletPipeBuilder<int>>();
        }
    }
}