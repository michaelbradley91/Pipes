using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
        ISimpleOutlet<TMessage> LeftOutlet { get; }
        ISimpleOutlet<TMessage> RightOutlet { get; }
    }

    public abstract class TwoOutletPipe<TMessage> : SimplePipe<TMessage>, ITwoOutletPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public ISimpleOutlet<TMessage> LeftOutlet { get; }
        public ISimpleOutlet<TMessage> RightOutlet { get; }

        protected TwoOutletPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> leftOutlet, ISimpleOutlet<TMessage> rightOutlet)
            : base(new[] {inlet}, new[] {leftOutlet, rightOutlet})
        {
            Inlet = inlet;
            LeftOutlet = leftOutlet;
            RightOutlet = rightOutlet;
        }
    }
}