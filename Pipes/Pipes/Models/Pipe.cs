namespace Pipes.Models
{
    public interface IPipe
    {
        int Capacity { get; }
        IInlet Inlet { get; }
        IOutlet Outlet { get; }
    }

    public class Pipe : IPipe
    {
        public int Capacity { get; private set; }
        public IInlet Inlet { get; private set; }
        public IOutlet Outlet { get; private set; }

        internal Pipe(int capacity)
        {
            Capacity = capacity;
            Inlet = new Inlet();
            Outlet = new Outlet();
        }
    }
}