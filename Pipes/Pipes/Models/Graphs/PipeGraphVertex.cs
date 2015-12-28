using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Models.Graphs
{
    public class PipeGraphVertex : Either<IPipe, Either<IInlet, IOutlet>>
    {
        public bool IsPipe => IsLeft;
        public bool IsInlet => IsRight && GetRight().IsLeft;
        public bool IsOutlet => IsRight && GetRight().IsRight;

        private PipeGraphVertex(IPipe left) : base(left) { }
        private PipeGraphVertex(Either<IInlet, IOutlet> right) : base(right) { }

        public static PipeGraphVertex Create(IPipe pipe)
        {
            return new PipeGraphVertex(pipe);
        }

        public static PipeGraphVertex Create(IInlet inlet)
        {
            return new PipeGraphVertex(new Either<IInlet, IOutlet>(inlet));
        }

        public static PipeGraphVertex Create(IOutlet outlet)
        {
            return new PipeGraphVertex(new Either<IInlet, IOutlet>(outlet));
        }

        public IPipe GetPipe()
        {
            return GetLeft();
        }

        public IInlet GetInlet()
        {
            return GetRight().GetLeft();
        }

        public IOutlet GetOutlet()
        {
            return GetRight().GetRight();
        }
    }
}
