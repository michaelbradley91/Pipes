using System;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Models.Lets
{
    public interface IAdapterInlet<TMessage> : IInlet<TMessage>
    {
    }

    public class AdapterInlet<TMessage> : Inlet<TMessage>, IAdapterInlet<TMessage>
    {
        public AdapterInlet(IPromised<IPipe> promisedPipe) : base(promisedPipe)
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
