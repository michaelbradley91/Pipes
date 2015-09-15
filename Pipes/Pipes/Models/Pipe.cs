namespace Pipes.Models
{
    public interface IPipe
    {
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