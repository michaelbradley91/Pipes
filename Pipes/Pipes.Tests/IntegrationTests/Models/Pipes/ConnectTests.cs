using System;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;

namespace Pipes.Tests.IntegrationTests.Models.Pipes
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        public void ConnectingTheOutletOfAPipeSystemWithMessages_ToAPipeSystemWithSpareCapacity_CausesTheMessagesToFlowThroughTheOutlet()
        {
            // Arrange
            var sourceCapacityPipe1 = PipeBuilder.New.CapacityPipe<int>().WithCapacity(1).Build();
            var sourceCapacityPipe2 = PipeBuilder.New.CapacityPipe<int>().WithCapacity(2).Build();
            var sourceEitherInletPipe = PipeBuilder.New.EitherInletPipe<int>().WithPrioritisingTieBreaker().Build();
            sourceEitherInletPipe.LeftInlet.ConnectTo(sourceCapacityPipe1.Outlet);
            sourceEitherInletPipe.RightInlet.ConnectTo(sourceCapacityPipe2.Outlet);

            sourceCapacityPipe1.Inlet.Send(1);
            sourceCapacityPipe2.Inlet.Send(2);
            sourceCapacityPipe2.Inlet.Send(3);

            var targetCapacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(3).Build();

            // Act
            sourceEitherInletPipe.Outlet.ConnectTo(targetCapacityPipe.Inlet);

            // Assert
            targetCapacityPipe.Outlet.Receive().Should().Be(1);
            targetCapacityPipe.Outlet.Receive().Should().Be(2);
            targetCapacityPipe.Outlet.Receive().Should().Be(3);

            try
            {
                targetCapacityPipe.Outlet.ReceiveImmediately();
                Assert.Fail("The pipe system should not have had any more messages");
            }
            catch (InvalidOperationException)
            {
                // Ignored
            }
        }
    }
}
