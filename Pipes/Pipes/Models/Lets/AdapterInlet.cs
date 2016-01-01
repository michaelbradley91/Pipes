using System;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public interface IAdapterInlet<TMessage> : IInlet<TMessage>
    {
    }

    public class AdapterInlet<TMessage> : Inlet<TMessage>, IAdapterInlet<TMessage>
    {
        public AdapterInlet(Lazy<IPipe> pipe, SharedResource sharedResource) : base(pipe, sharedResource)
        {
        }

        public override bool CanConnect()
        {
            return ConnectedOutlet == null;
        }

        public override Func<TMessage> FindSender()
        {
            return ConnectedOutlet?.Pipe.FindSender(ConnectedOutlet);
        }
    }
}
