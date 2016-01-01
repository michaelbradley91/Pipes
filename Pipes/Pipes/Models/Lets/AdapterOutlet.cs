using System;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public interface IAdapterOutlet<TMessage> : IOutlet<TMessage>
    {
    }

    public class AdapterOutlet<TMessage> : Outlet<TMessage>, IAdapterOutlet<TMessage>
    {
        public AdapterOutlet(Lazy<IPipe> pipe, SharedResource sharedResource) : base(pipe, sharedResource)
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
