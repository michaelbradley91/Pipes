﻿using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.Builders
{
    [TestFixture]
    public class SplittingPipeBuilderTests
    {
        private ISplittingPipeBuilder<int> splittingPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            splittingPipeBuilder = new SplittingPipeBuilder<int>();
        }

        [Test]
        public void Build_ReturnsASplittingPipe()
        {
            // Act
            var pipe = splittingPipeBuilder.Build();

            // Assert
            pipe.Should().NotBeNull();
        }
    }
}