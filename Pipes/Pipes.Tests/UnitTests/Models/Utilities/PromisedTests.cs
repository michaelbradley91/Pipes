using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Models.Utilities;

namespace Pipes.Tests.UnitTests.Models.Utilities
{
    [TestFixture]
    public class PromisedTests
    {
        private IPromised<int> promised;

        [SetUp]
        public void SetUp()
        {
            promised = new Promised<int>();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void GetPromisedObject_BeforeThePromiseIsFulfilled_ThrowsAnException()
        {
            // Act
            promised.GetPromisedObject();
        }

        [Test]
        public void Fulfill_SetsThePromisedObject()
        {
            // Arrange
            const int promise = 102;

            // Act
            promised.Fulfill(promise);

            // Assert
            promised.GetPromisedObject().Should().Be(promise);
        }

        [Test]
        public void Fulfill_ReturnsTheObjectPassedIn()
        {
            // Arrange
            const int promise = 102;

            // Act
            var result = promised.Fulfill(promise);

            // Assert
            result.Should().Be(promise);
        }
    }
}
