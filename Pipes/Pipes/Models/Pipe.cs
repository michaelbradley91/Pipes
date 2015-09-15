namespace Pipes.Models
{
    public interface IPipe
    {
        int Capacity { get; }
    }

    public class Pipe : IPipe
    {
        public int Capacity { get; private set; }

        internal Pipe(int capacity)
        {
            Capacity = capacity;
        }
    }
}