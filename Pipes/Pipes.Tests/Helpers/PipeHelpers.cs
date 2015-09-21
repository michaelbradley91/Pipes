﻿using System.Collections.Generic;
using Moq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class PipeHelpers
    {
        public static Mock<IPipe<T>> CreateMockPipe<T>(int numberOfInlets = 1, int numberOfOutlets = 1)
        {
            var pipe = new Mock<IPipe<T>>();

            var inlets = new List<Inlet<T>>();
            for (var i = 0; i < numberOfOutlets; i++)
            {
                inlets.Add(InletHelpers.CreateInlet(pipe.Object));
            }

            var outlets = new List<Outlet<T>>();
            for (var i = 0; i < numberOfOutlets; i++)
            {
                outlets.Add(OutletHelpers.CreateOutlet(pipe.Object));
            }

            pipe.Setup(p => p.Inlets).Returns(inlets);
            pipe.Setup(p => p.Outlets).Returns(outlets);

            return pipe;
        }
    }
}
