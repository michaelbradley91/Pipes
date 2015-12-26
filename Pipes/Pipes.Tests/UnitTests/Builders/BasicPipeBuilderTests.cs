using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class BasicPipeBuilderTests
    {
        private IBasicPipeBuilder<int> basicPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            basicPipeBuilder = new BasicPipeBuilder<int>();
        }

        [Test]
        public void Build_ReturnsABasicPipe()
        {
            // Act
            var pipe = basicPipeBuilder.Build();

            // Assert
            pipe.GetType().Should().BeAssignableTo<IBasicPipe<int>>();
        }
    }
}