using NUnit.Framework;
using System;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class EitherInletPipeBuilderTests
    {
        private IEitherInletPipeBuilder<int> eitherInletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            eitherInletPipeBuilder = new EitherInletPipeBuilder<int>();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            eitherInletPipeBuilder.WithRandomisingTieBreaker(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            eitherInletPipeBuilder.WithRandomisingTieBreaker(2);
        }
    }
}