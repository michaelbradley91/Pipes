namespace Pipes.Models
{
    public interface IInlet<TMessageType>
    {
    }

    public class Inlet<TMessageType> : IInlet<TMessageType>
    {
        internal Inlet()
        {
        }
    }
}