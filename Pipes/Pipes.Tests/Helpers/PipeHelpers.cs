using System.Collections.Generic;
using Moq;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Tests.Helpers
{
    public static class PipeHelpers
    {
        public static Mock<IPipe> CreateMockPipe<T>(int numberOfInlets = 1, int numberOfOutlets = 1)
        {
            var pipe = new Mock<IPipe>();

            var inlets = new List<SimpleInlet<T>>();
            for (var i = 0; i < numberOfOutlets; i++)
            {
                inlets.Add(InletHelpers.CreateInlet<T>(pipe.Object));
            }

            var outlets = new List<SimpleOutlet<T>>();
            for (var i = 0; i < numberOfOutlets; i++)
            {
                outlets.Add(OutletHelpers.CreateOutlet<T>(pipe.Object));
            }
            
            pipe.Setup(p => p.AllInlets).Returns(inlets);
            pipe.Setup(p => p.AllOutlets).Returns(outlets);

            return pipe;
        }
    }
}
