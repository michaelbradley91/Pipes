using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Constants;

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

            var targetCapacityPipe1 = PipeBuilder.New.CapacityPipe<int>().WithCapacity(2).Build();
            var targetCapacityPipe2 = PipeBuilder.New.CapacityPipe<int>().WithCapacity(1).Build();
            var targetEitherOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().WithPrioritisingTieBreaker().Build();
            targetEitherOutletPipe.LeftOutlet.ConnectTo(targetCapacityPipe1.Inlet);
            targetEitherOutletPipe.RightOutlet.ConnectTo(targetCapacityPipe2.Inlet);

            // Act
            sourceEitherInletPipe.Outlet.ConnectTo(targetEitherOutletPipe.Inlet);
            sourceEitherInletPipe.Outlet.Disconnect();

            // Assert
            var targetEitherInletPipe = PipeBuilder.New.EitherInletPipe<int>().WithPrioritisingTieBreaker().Build();
            targetEitherInletPipe.LeftInlet.ConnectTo(targetCapacityPipe1.Outlet);
            targetEitherInletPipe.RightInlet.ConnectTo(targetCapacityPipe2.Outlet);

            targetEitherInletPipe.Outlet.Receive().Should().Be(1);
            targetEitherInletPipe.Outlet.Receive().Should().Be(2);
            targetEitherInletPipe.Outlet.Receive().Should().Be(3);

            try
            {
                targetEitherInletPipe.Outlet.ReceiveImmediately();
                Assert.Fail("The pipe system should not have had any more messages");
            }
            catch (InvalidOperationException)
            {
                // Ignored
            }
        }
    }
}
