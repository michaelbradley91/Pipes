using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
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
            builder.Should().BeAssignableTo<BasicPipeBuilder<int>>();
        }

        [Test]
        public void CapacityPipe_ReturnsACapacityPipePipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.CapacityPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<CapacityPipeBuilder<int>>();
        }

        [Test]
        public void EitherInletPipe_ReturnsAnEitherInletPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.EitherInletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<EitherInletPipeBuilder<int>>();
        }

        [Test]
        public void EitherOutletPipe_ReturnsAnEitherOutletPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.EitherOutletPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<EitherOutletPipeBuilder<int>>();
        }

        [Test]
        public void SplittingPipe_ReturnsASplittingPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.SplittingPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<SplittingPipeBuilder<int>>();
        }

        [Test]
        public void TransformPipe_ReturnsATransformPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.TransformPipe<int, string>();

            // Assert
            builder.Should().BeAssignableTo<TransformPipeBuilder<int, string>>();
        }

        [Test]
        public void SinkPipe_ReturnsASinkPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.SinkPipe<int>();

            // Assert
            builder.Should().BeAssignableTo<SinkPipeBuilder<int>>();
        }

        [Test]
        public void SourcePipe_ReturnsASourcePipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.SourcePipe<int>();

            // Assert
            builder.Should().BeAssignableTo<SourcePipeBuilder<int>>();
        }

        [Test]
        public void ValvedPipe_ReturnsAValvedPipeBuilder()
        {
            // Act
            var builder = PipeBuilder.New.ValvedPipe<string, int>();

            // Assert
            builder.Should().BeAssignableTo<ValvedPipeBuilder<string, int>>();
        }
    }
}