namespace Pipes.Models
{
    public interface IOutlet<out TMessageType>
    {
        TMessageType Receive();
    }

    public class Outlet<TMessageType> : IOutlet<TMessageType>
    {
        internal Outlet()
        {
        }

        public TMessageType Receive()
        {
            throw new System.NotImplementedException();
        }
    }
}