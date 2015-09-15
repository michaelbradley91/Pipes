namespace Pipes.Models
{
    public interface IOutlet<TMessageType>
    {
    }

    public class Outlet<TMessageType> : IOutlet<TMessageType>
    {
        internal Outlet()
        {
        }
    }
}