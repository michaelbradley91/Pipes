using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class SourcePipeTests
    {
        private const int repeatedMessage = 3;
        private ISourcePipe<int> sourcePipe;

        [SetUp]
        public void SetUp()
        {
            sourcePipe = PipeBuilder.New.SourcePipe<int>().WithMessageProducer(() => repeatedMessage).Build();
        }

        [Test]
        public void SourcePipe_IsASimplePipe()
        {
            // Assert
            sourcePipe.Should().BeAssignableTo<SimplePipe<int>>();
        }
        
        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindReceiver_AsThereAreNoInletsOnThePipe_ThrowsAnExceptiojn()
        {
            // Arrange
            var inlet = new Mock<IInlet<int>>().Object;

            // Act
            sourcePipe.FindReceiver(inlet);
        }
        
        [Test]
        public void FindSender_AlwaysReturnsASenderThatInvokesTheMessageProducer()
        {
            // Act
            var sender = sourcePipe.FindSender(sourcePipe.Outlet);

            // Assert
            sender.Should().NotBeNull();
            sender().Should().Be(repeatedMessage);
        }
    }
}
