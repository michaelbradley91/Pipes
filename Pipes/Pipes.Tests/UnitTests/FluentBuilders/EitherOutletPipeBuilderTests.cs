using NUnit.Framework;
using System;
using Pipes.Builders;

namespace Pipes.Tests.UnitTests.FluentBuilders
{
    [TestFixture]
    public class EitherOutletPipeBuilderTests
    {
        private IEitherOutletPipeBuilder<int> eitherOutletPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            eitherOutletPipeBuilder = new EitherOutletPipeBuilder<int>();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            eitherOutletPipeBuilder.WithRandomisingTieBreaker(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRandomisingTieBreaker_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            eitherOutletPipeBuilder.WithRandomisingTieBreaker(2);
        }
    }
}