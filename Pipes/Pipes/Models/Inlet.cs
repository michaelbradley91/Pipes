namespace Pipes.Models
{
    public interface IInlet<in TMessageType>
    {
        void Send(TMessageType message);
    }

    public class Inlet<TMessageType> : IInlet<TMessageType>
    {
        internal Inlet()
        {
        }

        public void Send(TMessageType message)
        {
            throw new System.NotImplementedException();
        }
    }
}