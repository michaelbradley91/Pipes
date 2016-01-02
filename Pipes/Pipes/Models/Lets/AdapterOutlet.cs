using System;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Models.Lets
{
    public interface IAdapterOutlet<TMessage> : IOutlet<TMessage>
    {
    }

    public class AdapterOutlet<TMessage> : Outlet<TMessage>, IAdapterOutlet<TMessage>
    {
        public AdapterOutlet(IPromised<IPipe> promisedPipe) : base(promisedPipe)
        {
        }

        public override bool CanConnect()
        {
            return ConnectedInlet == null;
        }

        public override Action<TMessage> FindReceiver()
        {
            return ConnectedInlet?.Pipe.FindReceiver(ConnectedInlet);
        }
    }
}
