using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class TransformPipeBuilderWithMapTests
    {        
        [Test]
        public void TheMapPassedIntoTheConstructor_IsSetOnTheMapProperty()
        {
            // Arrange
            Func<int, string> map = x => x.ToString();

            // Act
            var transformPipeWithMapBuilder = new TransformPipeWithMapBuilder<int, string>(map);

            // Assert
            transformPipeWithMapBuilder.Should().NotBeNull();
            transformPipeWithMapBuilder.Map.Should().Be(map);
        }

        [Test]
        public void Build_GivenAMapInTheConstructor_ProducesATransformPipeWithThatMap()
        {
            // Arrange
            Func<int, string> map = x => x.ToString();
            var transformPipeWithMapBuilder = new TransformPipeWithMapBuilder<int, string>(map);

            // Act
            var pipe = transformPipeWithMapBuilder.Build();

            // Assert
            pipe.Map.Should().Be(map);
            pipe.Inlet.Should().NotBeNull();
        }

        [Test]
        public void Build_WithTheMapSetInTheProperty_ProducesATransformPipeWithThatMap()
        {
            // Arrange
            Func<int, string> map = x => x.ToString();
            var transformPipeWithMapBuilder = new TransformPipeWithMapBuilder<int, string>(x => "") {Map = map};
            // Act
            var pipe = transformPipeWithMapBuilder.Build();

            // Assert
            pipe.Map.Should().Be(map);
            pipe.Inlet.Should().NotBeNull();
        }
    }
}