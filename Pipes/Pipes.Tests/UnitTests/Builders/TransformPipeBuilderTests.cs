using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TransformPipeBuilderTests
    {
        private ITransformPipeBuilder<int, string> transformPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            transformPipeBuilder = new TransformPipeBuilder<int, string>();
        }

        [Test]
        public void WithMap_ReturnsATransformPipeWithMapBuilder_WithTheSpecifiedMap()
        {
            // Arrange
            Func<int, string> map = x => x.ToString();

            // Act
            var transformPipeWithMapBuilder = transformPipeBuilder.WithMap(map);

            // Assert
            transformPipeWithMapBuilder.Should().NotBeNull();
            transformPipeWithMapBuilder.Map.Should().Be(map);
        }
    }
}