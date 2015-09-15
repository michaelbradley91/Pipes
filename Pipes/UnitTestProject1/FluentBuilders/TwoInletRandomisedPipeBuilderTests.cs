using System;
using NUnit.Framework;
using Pipes.FluentBuilders;

namespace Pipes.Tests.FluentBuilders
{
    [TestFixture]
    public class TwoInletRandomisedPipeBuilderTests
    {
        private ITwoInletRandomisedPipeBuilder twoInletRandomisedPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            twoInletRandomisedPipeBuilder = new TwoInletRandomisedPipeBuilder();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithLeftProbability_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            twoInletRandomisedPipeBuilder.WithLeftProbability(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithLeftProbability_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            twoInletRandomisedPipeBuilder.WithLeftProbability(2);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRightProbability_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            twoInletRandomisedPipeBuilder.WithRightProbability(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRightProbability_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            twoInletRandomisedPipeBuilder.WithRightProbability(2);
        }
    }
}
