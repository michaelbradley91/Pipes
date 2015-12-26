using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Models.Pipes
{
    [TestFixture]
    public class PipeTests
    {
        private DummyPipe dummyPipe;

        [SetUp]
        public void SetUp()
        {
            dummyPipe = new DummyPipe();
        }

        [Test]
        public void FindReceiver_GivenAnInletBelongingToThePipe_DoesNotThrowAnException()
        {
            // Arrange
            var inlet = dummyPipe.Inlet.Object;

            // Act
            var receiver = dummyPipe.FindReceiver(inlet, true);

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
            // Arrange
            var outlet = dummyPipe.Outlet.Object;

            // Act
            var sender = dummyPipe.FindSender(outlet, true);

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

        private class DummyPipe : Pipe<int>
        {
            public readonly Mock<IInlet<int>> Inlet = new Mock<IInlet<int>>();
            public readonly Mock<IOutlet<int>> Outlet = new Mock<IOutlet<int>>();

            public override IReadOnlyCollection<IInlet<int>> Inlets => new List<IInlet<int>> {Inlet.Object};
            public override IReadOnlyCollection<IOutlet<int>> Outlets => new List<IOutlet<int>> { Outlet.Object };

            protected override Action<int> FindReceiver(IInlet<int> inletSendingMessage)
            {
                return i => { };
            }

            protected override Func<int> FindSender(IOutlet<int> outletReceivingMessage)
            {
                return () => 3;
            }
        }
    }
}
