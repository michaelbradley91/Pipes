using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Tests.Helpers;

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
            inlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            outlet = new Mock<ISimpleOutlet<int>>();
            outlet.SetupGet(i => i.SharedResource).Returns(SharedResourceHelpers.CreateSharedResource());

            dummyPipe = new DummyPipe(inlet.Object, outlet.Object);
        }

        [Test]
        public void FindReceiver_GivenAnInletBelongingToThePipe_DoesNotThrowAnException()
        {
            // Act
            var receiver = dummyPipe.FindReceiver(inlet.Object, true);

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
            dummyPipe.FindReceiver(inlet.Object, true);
        }

        [Test]
        public void FindReceiver_GivenAnInletThatDoesNotBelongToThePipeButToldNotToCheckTheInlet_DoesNotThrowAnException()
        {
            // Arrange
            var inlet = new Mock<IInlet<int>>();

            // Act
            var receiver = dummyPipe.FindReceiver(inlet.Object, false);

            // Assert
            receiver.Should().NotBeNull();
        }

        [Test]
        public void FindSender_GivenAnOutletBelongingToThePipe_DoesNotThrowAnException()
        {
            // Act
            var sender = dummyPipe.FindSender(outlet.Object, true);

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
            dummyPipe.FindSender(outlet.Object, true);
        }

        [Test]
        public void FindSender_GivenAnOutletThatDoesNotBelongToThePipeButToldNotToCheckTheOutlet_DoesNotThrowAnException()
        {
            // Arrange
            var outlet = new Mock<IOutlet<int>>();

            // Act
            var sender = dummyPipe.FindSender(outlet.Object, false);

            // Assert
            sender.Should().NotBeNull();
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
