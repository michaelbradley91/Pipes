using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class PipeBuilderTests
    {
        [Test]
        public void New_ReturnsAPipeBuilder()
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
            var builder = PipeBuilder.New.BasicPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IBasicPipeBuilder<int>>();
        }

        [Test]
        public void CapacityPipe_ReturnsACapacityPipePipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.CapacityPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<ICapacityPipeBuilder<int>>();
        }

        [Test]
        public void EitherInletPipe_ReturnsAnEitherInletPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.EitherInletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IEitherInletPipeBuilder<int>>();
        }

        [Test]
        public void EitherOutletPipe_ReturnsAnEitherOutletPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.EitherOutletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<IEitherOutletPipeBuilder<int>>();
        }
    }
}