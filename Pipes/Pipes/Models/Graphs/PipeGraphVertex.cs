using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Models.Graphs
{
    public class PipeGraphVertex<TMessage> : Either<IPipe<TMessage>, Either<IInlet<TMessage>, IOutlet<TMessage>>>
    {
        public bool IsPipe => IsLeft;
        public bool IsInlet => IsRight && GetRight().IsLeft;
        public bool IsOutlet => IsRight && GetRight().IsRight;

        private PipeGraphVertex(IPipe<TMessage> left) : base(left) { }
        private PipeGraphVertex(Either<IInlet<TMessage>, IOutlet<TMessage>> right) : base(right) { }

        public static PipeGraphVertex<TMessage> Create(IPipe<TMessage> pipe)
        {
            return new PipeGraphVertex<TMessage>(pipe);
        }

        public static PipeGraphVertex<TMessage> Create(IInlet<TMessage> inlet)
        {
            return new PipeGraphVertex<TMessage>(new Either<IInlet<TMessage>, IOutlet<TMessage>>(inlet));
        }

        public static PipeGraphVertex<TMessage> Create(IOutlet<TMessage> outlet)
        {
            return new PipeGraphVertex<TMessage>(new Either<IInlet<TMessage>, IOutlet<TMessage>>(outlet));
        }

        public IPipe<TMessage> GetPipe()
        {
            return GetLeft();
        }

        public IInlet<TMessage> GetInlet()
        {
            return GetRight().GetLeft();
        }

        public IOutlet<TMessage> GetOutlet()
        {
            return GetRight().GetRight();
        }
    }
}
