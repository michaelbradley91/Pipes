using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class PipeTests
    {
        private Mock<ISimpleInlet<int>> inlet;
        private Mock<ISimpleOutlet<int>> outlet;
        private DummyPipe dummyPipe;

        [SetUp]
        public void SetUp()
        {
            inlet = new Mock<ISimpleInlet<int>>();
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResource.Create());

            outlet = new Mock<ISimpleOutlet<int>>();
            outlet.SetupGet(i => i.SharedResource).Returns(SharedResource.Create());

            dummyPipe = new DummyPipe(inlet.Object, outlet.Object);
        }

        [Test]
        public void FindReceiver_GivenAnInletBelongingToThePipe_DoesNotThrowAnException()
        {
            // Act
            var receiver = dummyPipe.FindReceiver(inlet.Object);

            // Assert
            receiver.Should().NotBeNull();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindReceiver_GivenAnInletThatDoesNotBelongToThePipeAndToldToCheckTheInlet_ThrowsAnException()
        {
            // Arrange
            var inlet = new Mock<IInlet<int>>();

            // Act
            dummyPipe.FindReceiver(inlet.Object);
        }

        [Test]
        public void FindSender_GivenAnOutletBelongingToThePipe_DoesNotThrowAnException()
        {
            // Act
            var sender = dummyPipe.FindSender(outlet.Object);

            // Assert
            sender.Should().NotBeNull();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [Test]
        public void FindSender_GivenAnOutletThatDoesNotBelongToThePipeAndToldToCheckTheOutlet_ThrowsAnException()
        {
            // Arrange
            var outlet = new Mock<IOutlet<int>>();

            // Act
            dummyPipe.FindSender(outlet.Object);
        }

        private class DummyPipe : Pipe
        {
            public DummyPipe(IInlet<int> inlet, IOutlet<int> outlet)
                : base(new[] {inlet}, new[] {outlet})
            {
            }

            protected override Action<T> FindReceiverFor<T>(IInlet<T> inletSendingMessage)
            {
                return i => { };
            }

            protected override Func<T> FindSenderFor<T>(IOutlet<T> outletReceivingMessage)
            {
                return () => (T)(object)3;
            }
        }
    }
}
