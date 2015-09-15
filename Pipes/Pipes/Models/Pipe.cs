namespace Pipes.Models
{
    public interface IPipe<TMessageType>
    {
        int Capacity { get; }
        IInlet<TMessageType> Inlet { get; }
        IOutlet<TMessageType> Outlet { get; }
    }

    public class Pipe<TMessageType> : IPipe<TMessageType>
    {
        public int Capacity { get; private set; }
        public IInlet<TMessageType> Inlet { get; private set; }
        public IOutlet<TMessageType> Outlet { get; private set; }

        internal Pipe(int capacity)
        {
            Capacity = capacity;
            Inlet = new Inlet<TMessageType>();
            Outlet = new Outlet<TMessageType>();
        }
    }
}