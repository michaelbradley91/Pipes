using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IStraightPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
        ISimpleOutlet<TMessage> Outlet { get; }
    }

    public abstract class StraightPipe<TMessage> : SimplePipe<TMessage>, IStraightPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public ISimpleOutlet<TMessage> Outlet { get; }

        protected StraightPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> outlet)
            : base(new[] {inlet}, new[] {outlet})
        {
            Inlet = inlet;
            Outlet = outlet;
        }
    }
}
