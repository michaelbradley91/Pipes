using System;
using NUnit.Framework;
using Pipes.FluentBuilders;

namespace Pipes.Tests.FluentBuilders
{
    [TestFixture]
    public class TwoOutletRandomisedPipeBuilderTests
    {
        private ITwoOutletRandomisedPipeBuilder twoOutletRandomisedPipeBuilder;

        [SetUp]
        public void SetUp()
        {
            twoOutletRandomisedPipeBuilder = new TwoOutletRandomisedPipeBuilder();
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithLeftProbability_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            twoOutletRandomisedPipeBuilder.WithLeftProbability(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithLeftProbability_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            twoOutletRandomisedPipeBuilder.WithLeftProbability(2);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRightProbability_GivenANegativeProbability_ThrowsAnArgumentOutOfRangeException()
        {
            twoOutletRandomisedPipeBuilder.WithRightProbability(-4);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Test]
        public void WithRightProbability_GivenAProbabilityGreaterThanOne_ThrowsAnArgumentOutOfRangeException()
        {
            twoOutletRandomisedPipeBuilder.WithRightProbability(2);
        }
    }
}
